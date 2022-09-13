/*
 *  Copyright 2013 Vitaliy Fedorchenko
 *
 *  Licensed under VideoConverter Source Code Licence (see LICENSE file).
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS 
 *  OF ANY KIND, either express or implied.
 */ 
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace NReco.VideoConverter {
	
	/// <summary>
	/// Represents async live media conversion task. 
	/// </summary>
	public class ConvertLiveMediaTask {

		Stream Input;
		Stream Output;
		FFMpegConverter FFMpegConv;
		string FFMpegToolArgs;
		
		Process FFMpegProcess;
		Thread CopyToStdInThread;
		Thread CopyFromStdOutThread;

		public EventHandler OutputDataReceived;

		string lastErrorLine;
		FFMpegProgress ffmpegProgress = null;
		long WriteBytesCount = 0;

		Exception lastStreamException = null;

		internal ConvertLiveMediaTask(FFMpegConverter ffmpegConv, string ffMpegArgs, Stream inputStream, Stream outputStream, FFMpegProgress progress) {
			Input = inputStream;
			Output = outputStream;
			FFMpegConv = ffmpegConv;
			FFMpegToolArgs = ffMpegArgs;
			ffmpegProgress = progress;
		}

		/// <summary>
		/// Start live stream conversion
		/// </summary>
		public void Start() {
			lastStreamException = null;
			var exePath = FFMpegConv.GetFFMpegExePath();
			if (!File.Exists(exePath))
				throw new FileNotFoundException("Cannot find ffmpeg tool: " + exePath);

#if TRIAL
			License.L.Check();
#endif

			var startInfo = new ProcessStartInfo(exePath, "-stdin "+FFMpegToolArgs);
#if !NETSTANDARD2_0
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
#endif
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
#if !NETSTANDARD2_0
			startInfo.StandardOutputEncoding = System.Text.Encoding.Default;
#else
			startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
#endif
			FFMpegConv.InitStartInfo(startInfo);

			FFMpegProcess = Process.Start(startInfo);
			if (FFMpegConv.FFMpegProcessPriority!=ProcessPriorityClass.Normal)
				FFMpegProcess.PriorityClass = FFMpegConv.FFMpegProcessPriority;

			lastErrorLine = null;
			ffmpegProgress.Reset();

			FFMpegProcess.ErrorDataReceived += (o, args) => {
				if (args.Data == null)
					return;
				lastErrorLine = args.Data;

				ffmpegProgress.ParseLine(args.Data);

				FFMpegConv.FFMpegLogHandler(args.Data);
			};
			FFMpegProcess.BeginErrorReadLine();

			if (Input != null) {
				CopyToStdInThread = new Thread(new ThreadStart(CopyToStdIn));
				CopyToStdInThread.Start();
			} else {
				CopyToStdInThread = null;
			}

			if (Output != null) {
				CopyFromStdOutThread = new Thread(new ThreadStart(CopyFromStdOut));
				CopyFromStdOutThread.Start();
			} else {
				CopyFromStdOutThread = null;
			}
		}

		/// <summary>
		/// Write input data into conversion stream
		/// </summary>
		public void Write(byte[] buf, int offset, int count) {
			if (FFMpegProcess.HasExited) {
				if (FFMpegProcess.ExitCode!=0)
					throw new FFMpegException(FFMpegProcess.ExitCode, String.IsNullOrEmpty(lastErrorLine) ? "FFMpeg process has exited" : lastErrorLine );
				else 
					throw new FFMpegException(-1, "FFMpeg process has exited");
			} else { 
				FFMpegProcess.StandardInput.BaseStream.Write(buf, offset, count);
				FFMpegProcess.StandardInput.BaseStream.Flush();
				WriteBytesCount += count;
			}
		}

		/// <summary>
		/// Stop live stream conversion process
		/// </summary>
		public void Stop() {
			Stop(false);
		}

		/// <summary>
		/// Stop live stream conversion process and optionally force ffmpeg to quit
		/// </summary>
		/// <param name="forceFFMpegQuit">force FFMpeg to quit by sending 'q' command to stdin.</param>
		public void Stop(bool forceFFMpegQuit) {
			if (CopyToStdInThread != null) {
				CopyToStdInThread = null; // thread code checks this field and stops if it is = null
			}

			if (forceFFMpegQuit) {
				if (Input==null && WriteBytesCount==0 ) {
					// this means that input data is not provided from .NET code
					// lets send 'q' to stdin
					FFMpegProcess.StandardInput.WriteLine("q\n");
					FFMpegProcess.StandardInput.Close();
				} else {
					Abort();
				}
			} else {
				FFMpegProcess.StandardInput.BaseStream.Close();
			}

			Wait();
		}

		void OnStreamError(Exception ex, bool isStdinStdout) {
			// ignore errors related to writing to stdin / reading from stdout when process is exited
			if ((ex is IOException) && isStdinStdout)
				return; // ignore write to stdin / read from stdout errors
			lastStreamException = ex;
			Abort();
		}

		protected void CopyToStdIn() {
			var buf = new byte[64*1024];
			var copyToStdInThread = CopyToStdInThread;
			var ffmpegProcess = FFMpegProcess;
			var stdinStream = FFMpegProcess.StandardInput.BaseStream;

			while (true) {
				int read;
				try { 
					read = Input.Read(buf, 0, buf.Length);
				} catch (Exception ex) {
					OnStreamError(ex, false);
					return;
				}
				if (read<=0)
					break;

				// check for stop
				if (FFMpegProcess==null ||
					!Object.ReferenceEquals(copyToStdInThread,CopyToStdInThread) || !Object.ReferenceEquals(ffmpegProcess,FFMpegProcess))
					return;

				try { 
					stdinStream.Write(buf, 0, read);
					stdinStream.Flush();
				} catch (Exception ex) {
					OnStreamError(ex, true);
					return;
				}
				
			}
			FFMpegProcess.StandardInput.Close();
		}

		protected void CopyFromStdOut() {
			var buf = new byte[64 * 1024];
			var copyToStdOutThread = CopyFromStdOutThread;
			var stdoutStream = FFMpegProcess.StandardOutput.BaseStream;
			while (true) {
				// check for stop
				if (!Object.ReferenceEquals(copyToStdOutThread,CopyFromStdOutThread))
					break;
				int read;
				try {
					read = stdoutStream.Read(buf, 0, buf.Length);
				} catch (Exception ex) {
					OnStreamError(ex, true);
					return;
				}
				if (read > 0) {
					// check for stop
					if (!Object.ReferenceEquals(copyToStdOutThread,CopyFromStdOutThread))
						return;
					try { 
						Output.Write(buf, 0, read);
						Output.Flush();
					} catch (Exception ex) {
						OnStreamError(ex, false);
						return;
					}

					if (OutputDataReceived!=null)
						OutputDataReceived(this, EventArgs.Empty);

				} else {
					Thread.Sleep(30);
				}
			}
		}

		/// <summary>
		/// Wait until live stream conversion is finished (input stream ended)
		/// </summary>
		/// <remarks>
		/// Do not call "Wait" when input stream is not used and input data is provided using Write method
		/// </remarks>
		public void Wait() {
			FFMpegProcess.WaitForExit(Int32.MaxValue);
			if (CopyToStdInThread!=null)
				CopyToStdInThread = null;
			if (CopyFromStdOutThread!=null)
				CopyFromStdOutThread = null;
			
			if (FFMpegProcess.ExitCode != 0) {
				throw new FFMpegException(FFMpegProcess.ExitCode, lastErrorLine ?? "Unknown error");
			}
			if (lastStreamException!=null)
				throw new IOException(lastStreamException.Message, lastStreamException);
			FFMpegProcess.Close();

			ffmpegProgress.Complete();
		}

		/// <summary>
		/// Abort live stream conversions process
		/// </summary>
		public void Abort() {
			if (CopyToStdInThread!=null)
				CopyToStdInThread = null;
			if (CopyFromStdOutThread!=null)
				CopyFromStdOutThread = null;
			try { 
				FFMpegProcess.Kill();
			} catch (InvalidOperationException ex) {
				// lets ignore exception related to process state check
				Debug.WriteLine(ex);
			}
		}

		internal class StreamOperationContext {
			
			public Stream TargetStream { get; private set; }

			bool isInput;
			bool isRead;

			public bool Read { get { return isRead; } }
			public bool Write { get { return !isRead; } }

			public bool IsInput { get { return isInput; } }
			public bool IsOutput { get { return !isInput; } }

			internal StreamOperationContext(Stream stream, bool isInput, bool isRead) {
				TargetStream = stream;
				this.isInput = isInput;
				this.isRead = isRead;
			}
		}
	}
}
