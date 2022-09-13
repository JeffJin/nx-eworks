/*
 *  Copyright 2013-2014 Vitaliy Fedorchenko
 *
 *  Licensed under VideoConverter Source Code Licence (see LICENSE file).
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS 
 *  OF ANY KIND, either express or implied.
 */ 

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.IO.Compression;
#if !NETSTANDARD2_0
using System.Web;
#endif
using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;

namespace NReco.VideoConverter
{
	/// <summary>
	/// Video converter component (wrapper to FFMpeg process)
	/// </summary>
    public class FFMpegConverter 
    {
		/// <summary>
		/// Gets or sets path where FFMpeg tool is located
		/// </summary>
		/// <remarks>
		/// By default this property points to the folder where application assemblies are located.
		/// If WkHtmlToPdf tool files are not present PdfConverter expands them from DLL resources.
		/// </remarks>
		public string FFMpegToolPath { get; set; }

		/// <summary>
		/// Gets or sets FFMpeg tool EXE file name ('ffmpeg.exe' by default)
		/// </summary>
		public string FFMpegExeName { get; set; }

		/// <summary>
		/// Gets or sets maximum execution time for conversion process (null is by default - means no timeout)
		/// </summary>
		public TimeSpan? ExecutionTimeout { get; set; }

		/// <summary>
		/// Occurs when FFMpeg outputs media info (total duration, convert progress)
		/// </summary>
		public event EventHandler<ConvertProgressEventArgs> ConvertProgress;

		/// <summary>
		/// Occurs when log line is received from FFMpeg process
		/// </summary>
		public event EventHandler<FFMpegLogEventArgs> LogReceived;

		/// <summary>
		/// Gets or sets FFMpeg process priority (Normal by default)
		/// </summary>
		public ProcessPriorityClass FFMpegProcessPriority { get; set; }

		/// <summary>
		/// Gets or sets user credential used for starting FFMpeg process.
		/// </summary>
		/// <remarks>By default this property is null and FFMpeg process uses credential of parent process (application pool in case of ASP.NET).</remarks>
		public FFMpegUserCredential FFMpegProcessUser { get; set; }

		/// <summary>
		/// Gets or sets ffmpeg loglevel option (by default is "info").
		/// </summary>
		public string LogLevel { get; set; }

		private Process FFMpegProcess = null;

		/// <summary>
		/// Initializes a new instance of the FFMpegConverter class.
		/// </summary>
		/// <remarks>
		/// FFMpegConverter is NOT thread-safe. Separate instance should be used for each thread.
		/// </remarks>
		public FFMpegConverter() {
			FFMpegProcessPriority = ProcessPriorityClass.Normal;
			LogLevel = "info";

#if NETSTANDARD2_0
			FFMpegToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FFMpeg");
#if !LIGHT 
//			if (HHttpContext.Current != null)
//				FFMpegToolPath = HttpRuntime.AppDomainAppPath + "bin";
#endif		
			if (String.IsNullOrEmpty( FFMpegToolPath )) {
				FFMpegToolPath = Path.GetDirectoryName( typeof(FFMpegConverter).Assembly.Location );
			}
#endif
			FFMpegExeName = "ffmpeg.exe";
		}

		private void CopyStream(Stream inputStream, Stream outputStream, int bufSize) {
			var buf = new byte[bufSize];
			int read;
			while ((read = inputStream.Read(buf, 0, buf.Length)) > 0)
				outputStream.Write(buf, 0, read);
		}

		/// <summary>
		/// Converts media represented by local file and writes result to specified local file
		/// </summary>
		/// <param name="inputFile">local path to input media file</param>
		/// <param name="outputFile">local path to ouput media file</param>
		/// <param name="outputFormat">desired output format (like "mp4" or "flv")</param>
		public void ConvertMedia(string inputFile, string outputFile, string outputFormat) {
			ConvertMedia(inputFile, null, outputFile, outputFormat, null);
		}

		/// <summary>
		/// Converts media represented by local file and writes result to specified local file with specified settings.
		/// </summary>
		/// <param name="inputFile">local path to input media file</param>
		/// <param name="inputFormat">input format (null for automatic format suggestion)</param>
		/// <param name="outputFile">local path to output media file</param>
		/// <param name="outputFormat">output media format</param>
		/// <param name="settings">explicit convert settings</param>
		public void ConvertMedia(string inputFile, string inputFormat, string outputFile, string outputFormat, ConvertSettings settings) {
			if (inputFile == null)
				throw new ArgumentNullException("inputFile");
			if (outputFile == null)
				throw new ArgumentNullException("outputFile");
			if (File.Exists(inputFile) && String.IsNullOrEmpty(Path.GetExtension(inputFile)) && inputFormat==null)
				throw new Exception("Input format is required for file without extension");
			if (String.IsNullOrEmpty(Path.GetExtension(outputFile)) && outputFormat == null)
				throw new Exception("Output format is required for file without extension");

			var input = new Media() { Filename = inputFile, Format = inputFormat };
			var output = new Media() { Filename = outputFile, Format = outputFormat };
			ConvertMedia(input, output, settings ?? new ConvertSettings() );
		}

		/// <summary>
		/// Converts media represented by local file and writes result to specified stream
		/// </summary>
		/// <param name="inputFile">local path to input media file</param>
		/// <param name="outputStream">output stream</param>
		/// <param name="outputFormat">output media format</param>
		public void ConvertMedia(string inputFile, Stream outputStream, string outputFormat) {
			ConvertMedia(inputFile, null, outputStream, outputFormat, null);
		}

		/// <summary>
		/// Converts media represented by local file and writes result to specified stream with specified convert settings.
		/// </summary>
		/// <param name="inputFile">local path to input media file</param>
		/// <param name="inputFormat">input format (null for automatic format suggestion)</param>
		/// <param name="outputStream">output stream</param>
		/// <param name="outputFormat">output media format</param>
		/// <param name="settings">convert settings</param>
		public void ConvertMedia(string inputFile, string inputFormat, Stream outputStream, string outputFormat, ConvertSettings settings) {
			if (inputFile == null)
				throw new ArgumentNullException("inputFile");
			if (File.Exists(inputFile) && String.IsNullOrEmpty(Path.GetExtension(inputFile)) && inputFormat == null)
				throw new Exception("Input format is required for file without extension");
			if (outputFormat == null)
				throw new ArgumentNullException("outputFormat");
			var input = new Media() { Filename = inputFile, Format = inputFormat };
			var output = new Media() { DataStream = outputStream, Format = outputFormat };
			ConvertMedia(input, output, settings ?? new ConvertSettings());
		}

		/// <summary>
		/// Converts several input files into one resulting output file. 
		/// </summary>
		/// <param name="inputs">one or more FFMpeg input specifiers</param>
		/// <param name="output">output file name</param>
		/// <param name="outputFormat">output file format (optional, can be null)</param>
		/// <param name="settings">output settings</param>
		public void ConvertMedia(FFMpegInput[] inputs, string output, string outputFormat, OutputSettings settings) {
			if (inputs==null || inputs.Length==0)
				throw new ArgumentException("At least one ffmpeg input should be specified");
			var lastInput = inputs[inputs.Length-1];

			var extraInputArgs = new StringBuilder();
			for (int i = 0; i < (inputs.Length-1); i++) {
				var input = inputs[i];
				if (input.Format != null)
					extraInputArgs.Append(" -f " + input.Format);
				if (input.CustomInputArgs!=null)
					extraInputArgs.AppendFormat(" {0} ", input.CustomInputArgs);
				extraInputArgs.AppendFormat(" -i {0} ", CommandArgParameter( input.Input ) );
			}

			var convSettings = new ConvertSettings();
			settings.CopyTo(convSettings);
			convSettings.CustomInputArgs = extraInputArgs.ToString() + lastInput.CustomInputArgs;

			ConvertMedia(lastInput.Input, lastInput.Format, output, outputFormat, convSettings );
		}


		/// <summary>
		/// Create a task for live stream conversion (real-time) without input source. Input data should be passed with Write method.
		/// </summary>
		/// <param name="inputFormat">input stream media format</param>
		/// <param name="outputStream">output media stream</param>
		/// <param name="outputFormat">output media format</param>
		/// <param name="settings">convert settings</param>
		/// <returns>instance of <see cref="NReco.VideoConverter.ConvertLiveMediaTask"/></returns>
		public ConvertLiveMediaTask ConvertLiveMedia(string inputFormat, Stream outputStream, string outputFormat, ConvertSettings settings) {
			return ConvertLiveMedia( (Stream)null, inputFormat, outputStream, outputFormat, settings);
		}

		/// <summary>
		/// Create a task for live stream conversion (real-time) that reads data from FFMpeg input source and write conversion result to output stream
		/// </summary>
		/// <param name="inputSource">input source string identifier (file path, UDP or TCP source, local video device name)</param>
		/// <param name="inputFormat">input stream media format</param>
		/// <param name="outputStream">output media stream</param>
		/// <param name="outputFormat">output media format</param>
		/// <param name="settings">convert settings</param>
		/// <returns>instance of <see cref="NReco.VideoConverter.ConvertLiveMediaTask"/></returns>
		public ConvertLiveMediaTask ConvertLiveMedia(string inputSource, string inputFormat, Stream outputStream, string outputFormat, ConvertSettings settings) {
			EnsureFFMpegLibs();
			var toolArgs = ComposeFFMpegCommandLineArgs(
								inputSource, inputFormat,
								"-", outputFormat, settings);
			return CreateLiveMediaTask(toolArgs, null, outputStream, settings);
		}

		/// <summary>
		/// Create a task for live stream conversion (real-time) that reads data from stream and writes conversion result to the file
		/// </summary>
		/// <param name="inputStream">input live stream (null if data is provided by calling "Write" method)</param>
		/// <param name="inputFormat">input stream media format</param>
		/// <param name="outputFile">output file path</param>
		/// <param name="outputFormat">output media format</param>
		/// <param name="settings">convert settings</param>
		/// <returns>instance of <see cref="NReco.VideoConverter.ConvertLiveMediaTask"/></returns>
		public ConvertLiveMediaTask ConvertLiveMedia(Stream inputStream, string inputFormat, string outputFile, string outputFormat, ConvertSettings settings) {
			EnsureFFMpegLibs();
			var toolArgs = ComposeFFMpegCommandLineArgs(
								"-", inputFormat,
								outputFile, outputFormat, settings);
			return CreateLiveMediaTask(toolArgs, inputStream, null, settings);
		}


		/// <summary>
		/// Create a task for live stream conversion (real-time) that reads data from input stream and write conversion result to output stream
		/// </summary>
		/// <param name="inputStream">input live stream (null if data is provided by calling "Write" method)</param>
		/// <param name="inputFormat">input stream media format</param>
		/// <param name="outputStream">output media stream</param>
		/// <param name="outputFormat">output media format</param>
		/// <param name="settings">convert settings</param>
		/// <returns>instance of <see cref="NReco.VideoConverter.ConvertLiveMediaTask"/></returns>
		public ConvertLiveMediaTask ConvertLiveMedia(Stream inputStream, string inputFormat, Stream outputStream, string outputFormat, ConvertSettings settings) {
			EnsureFFMpegLibs();
			var toolArgs = ComposeFFMpegCommandLineArgs(
								"-", inputFormat,
								"-", outputFormat, settings);
			return CreateLiveMediaTask(toolArgs, inputStream, outputStream, settings);
		}

		private ConvertLiveMediaTask CreateLiveMediaTask(string toolArgs, Stream inputStream, Stream outputStream, ConvertSettings settings) {
			var ffmpegProgress = new FFMpegProgress(OnConvertProgress, ConvertProgress!=null);
			if (settings != null) {
				ffmpegProgress.Seek = settings.Seek;
				ffmpegProgress.MaxDuration = settings.MaxDuration;
			}
			return new ConvertLiveMediaTask(this, toolArgs, inputStream, outputStream, ffmpegProgress);
		}


		/// <summary>
		/// Extract video thumbnail (first frame) from local video file
		/// </summary>
		/// <param name="inputFile">path to local video file</param>
		/// <param name="outputJpegStream">output stream for thumbnail in jpeg format</param>
		public void GetVideoThumbnail(string inputFile, Stream outputJpegStream) {
			GetVideoThumbnail(inputFile, outputJpegStream, null);
		}

		/// <summary>
		/// Extract video thumbnail (first frame) from local video file
		/// </summary>
		/// <param name="inputFile">path to local video file</param>
		/// <param name="outputFile">path to thumbnail jpeg file</param>
		public void GetVideoThumbnail(string inputFile, string outputFile) {
			GetVideoThumbnail(inputFile, outputFile, null);
		}

		/// <summary>
		/// Extract video frame from local video file at specified position
		/// </summary>
		/// <param name="inputFile">path to local video file</param>
		/// <param name="outputJpegStream">output stream for thumbnail in jpeg format</param>
		/// <param name="frameTime">video position (in seconds)</param>
		public void GetVideoThumbnail(string inputFile, Stream outputJpegStream, float? frameTime) {
			var input = new Media() { Filename = inputFile };
			var output = new Media() { DataStream = outputJpegStream, Format = "mjpeg" };
			var settings = new ConvertSettings() {
				VideoFrameCount = 1,
				Seek = frameTime,
				MaxDuration = 1
			};
			ConvertMedia(input, output, settings);
		}

		/// <summary>
		/// Extract video frame from local video file at specified position
		/// </summary>
		/// <param name="inputFile">path to local video file</param>
		/// <param name="outputFile">path to thumbnail jpeg file</param>
		/// <param name="frameTime">video position (in seconds)</param>
		public void GetVideoThumbnail(string inputFile, string outputFile, float? frameTime) {
			var input = new Media() { Filename = inputFile };
			var output = new Media() { Filename = outputFile, Format = "mjpeg" };
			var settings = new ConvertSettings() {
				VideoFrameCount = 1,
				Seek = frameTime,
				MaxDuration = 1
			};
			ConvertMedia(input, output, settings);
		}

		private string CommandArgParameter(string arg) {
			var sb = new StringBuilder();
			sb.Append('"');
			sb.Append(arg);
			sb.Append('"');
			return sb.ToString();
		}

		internal void InitStartInfo(ProcessStartInfo startInfo) {
			if (FFMpegProcessUser != null) {
				if (FFMpegProcessUser.Domain!=null)
					startInfo.Domain = FFMpegProcessUser.Domain;
				if (FFMpegProcessUser.UserName!=null)
					startInfo.UserName = FFMpegProcessUser.UserName;
				if (FFMpegProcessUser.Password!=null) {
#if NETSTANDARD2_0
					startInfo.PasswordInClearText = FFMpegProcessUser.Password;
#else
					startInfo.Password = FFMpegProcessUser.Password;
#endif
				}
			}
		}

		internal string GetFFMpegExePath() {
			return Path.Combine(FFMpegToolPath, FFMpegExeName);;
		}

		/// <summary>
		/// Concatenate several video files
		/// </summary>
		/// <param name="inputFiles">list of local video files</param>
		/// <param name="outputFile">path to contactenation result file</param>
		/// <param name="outputFormat">desired output format</param>
		/// <param name="settings">convert settings</param>
		/// <remarks>
		/// Note: all video files should have the same video frame size and audio stream.
		/// </remarks>
		public void ConcatMedia(string[] inputFiles, string outputFile, string outputFormat, ConcatSettings settings) {
			EnsureFFMpegLibs();
			
			var toolExe = GetFFMpegExePath();

#if TRIAL
			License.L.Check();
#endif

			if (!File.Exists(toolExe))
				throw new FileNotFoundException("Cannot find ffmpeg tool: " + toolExe);

			var inputFileArgs = new StringBuilder();
			foreach (var inputFile in inputFiles) {
				if (!File.Exists(inputFile))
					throw new FileNotFoundException("Cannot find input video file: " + inputFile);
				inputFileArgs.AppendFormat(" -i {0} ", CommandArgParameter( inputFile ) );
			}

			var outputFileArgs = new StringBuilder();
			ComposeFFMpegOutputArgs(outputFileArgs, outputFormat, settings);

			//c:\temp\ffmpeg>ffmpeg -i 09_frame.mjpeg -f lavfi -i aevalsrc=0 -shortest 09_frame.mp4
			//[0:0] [0:1] [1:0] [1:1]
			outputFileArgs.Append(" -filter_complex \"");
			outputFileArgs.AppendFormat("concat=n={0}", inputFiles.Length);
			if (settings.ConcatVideoStream)
				outputFileArgs.Append(":v=1");
			if (settings.ConcatAudioStream)
				outputFileArgs.Append(":a=1");

			if (settings.ConcatVideoStream)
				outputFileArgs.Append(" [v]");
			if (settings.ConcatAudioStream)
				outputFileArgs.Append(" [a]");

			outputFileArgs.Append("\" ");

			if (settings.ConcatVideoStream)
				outputFileArgs.Append(" -map \"[v]\" ");
			if (settings.ConcatAudioStream)
				outputFileArgs.Append(" -map \"[a]\" ");

			var toolArgs = String.Format("-y -loglevel {3} {0} {1} {2}",
					inputFileArgs.ToString(),
					outputFileArgs,
					CommandArgParameter( outputFile ),
					LogLevel
				);

			try {
				var startInfo = new ProcessStartInfo(toolExe, toolArgs);

#if !NETSTANDARD2_0
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
#endif
				startInfo.UseShellExecute = false;
				startInfo.CreateNoWindow = true;
				startInfo.WorkingDirectory = Path.GetDirectoryName(FFMpegToolPath);
				startInfo.RedirectStandardInput = true; // we can send 'q' to ffmpeg 
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;
				InitStartInfo(startInfo);

				if (FFMpegProcess!=null)
					throw new InvalidOperationException("FFMpeg process is already started");

				FFMpegProcess = Process.Start(startInfo);
				if (FFMpegProcessPriority!=ProcessPriorityClass.Normal)
					FFMpegProcess.PriorityClass = FFMpegProcessPriority;

				var lastErrorLine = String.Empty;
				var ffmpegProgress = new FFMpegProgress(OnConvertProgress, ConvertProgress!=null);
				if (settings!=null)
					ffmpegProgress.MaxDuration = settings.MaxDuration;

				FFMpegProcess.ErrorDataReceived += (o, args) => {
					if (args.Data == null)
						return;
					lastErrorLine = args.Data;

					ffmpegProgress.ParseLine(args.Data);
					FFMpegLogHandler(args.Data);
				};
				FFMpegProcess.OutputDataReceived += (o, args) => {
				};

				FFMpegProcess.BeginOutputReadLine();
				FFMpegProcess.BeginErrorReadLine();

				WaitFFMpegProcessForExit();

				if (FFMpegProcess.ExitCode != 0) {
					throw new FFMpegException(FFMpegProcess.ExitCode, lastErrorLine);
				}
				FFMpegProcess.Close();
				FFMpegProcess = null;

				ffmpegProgress.Complete();

			} catch (Exception ex) {
				Debug.WriteLine(ex);
				EnsureFFMpegProcessStopped();
				throw;
			} 
			
		}

		protected void WaitFFMpegProcessForExit() {
			if (FFMpegProcess == null) {
				throw new FFMpegException(-1, "FFMpeg process was aborted");
			}
			if (FFMpegProcess.HasExited)
				return;

			var timeout = ExecutionTimeout.HasValue ? (int)ExecutionTimeout.Value.TotalMilliseconds : Int32.MaxValue;
			// lets use Int32.MaxValue timeout as workaround for weird Process.WaitForExit bug that appears 
			// when both stdout and stderr are redirected (NullReferenceException in AsyncStreamReader.WaitUtilEOF - called only when timeout = -1) 
			if (!FFMpegProcess.WaitForExit(timeout)) {
				EnsureFFMpegProcessStopped();
				throw new FFMpegException(-2, String.Format("FFMpeg process exceeded execution timeout ({0}) and was aborted",ExecutionTimeout) );
			}


		}

		protected void EnsureFFMpegProcessStopped() {
			if (FFMpegProcess != null && !FFMpegProcess.HasExited) {
				// cleanup
				try {
					FFMpegProcess.Kill();
					FFMpegProcess = null;
				} catch (Exception ex) {
					Debug.WriteLine(ex);
				}
			}
		}


		protected void ComposeFFMpegOutputArgs(StringBuilder outputArgs, string outputFormat, OutputSettings settings) {
			if (settings==null)
				return;
			if (settings.MaxDuration != null)
				outputArgs.AppendFormat(CultureInfo.InvariantCulture, " -t {0}", settings.MaxDuration);
			if (outputFormat != null)
				outputArgs.AppendFormat(" -f {0} ", outputFormat);
			if (settings.AudioSampleRate != null)
				outputArgs.AppendFormat(" -ar {0}", settings.AudioSampleRate);
			if (settings.AudioCodec != null)
				outputArgs.AppendFormat(" -acodec {0}", settings.AudioCodec);
			if (settings.VideoFrameCount != null)
				outputArgs.AppendFormat(" -vframes {0}", settings.VideoFrameCount);
			if (settings.VideoFrameRate != null)
				outputArgs.AppendFormat(" -r {0}", settings.VideoFrameRate);
			if (settings.VideoCodec != null)
				outputArgs.AppendFormat(" -vcodec {0}", settings.VideoCodec);
			if (settings.VideoFrameSize != null)
				outputArgs.AppendFormat(" -s {0}", settings.VideoFrameSize);

			if (settings.CustomOutputArgs != null)
				outputArgs.AppendFormat(" {0} ", settings.CustomOutputArgs);
		}

		protected string ComposeFFMpegCommandLineArgs(string inputFile, string inputFormat, string outputFile, string outputFormat, ConvertSettings settings) {
			var inputArgs = new StringBuilder();

			if (settings.AppendSilentAudioStream) {
				inputArgs.Append(" -f lavfi -i aevalsrc=0 ");
			}

			if (settings.Seek != null)
				inputArgs.AppendFormat(CultureInfo.InvariantCulture, " -ss {0}", settings.Seek);
			if (inputFormat != null)
				inputArgs.Append(" -f " + inputFormat);
			if (settings.CustomInputArgs!=null)
				inputArgs.AppendFormat(" {0} ", settings.CustomInputArgs);

			var outputArgs = new StringBuilder();
			ComposeFFMpegOutputArgs(outputArgs, outputFormat, settings);

			if (settings.AppendSilentAudioStream) {
				outputArgs.Append(" -shortest ");
			}

			var toolArgs = String.Format("-y -loglevel {4} {0} -i {1} {2} {3}",
					inputArgs.ToString(), CommandArgParameter( inputFile ), 
					outputArgs.ToString(), CommandArgParameter( outputFile ),
					LogLevel);

			return toolArgs;
		}

		/// <summary>
		/// Invoke FFMpeg process with custom command line arguments
		/// </summary>
		/// <param name="ffmpegArgs">string with arguments</param>
		public void Invoke(string ffmpegArgs) {
			EnsureFFMpegLibs();
#if TRIAL
			License.L.Check();
#endif

			try {
				var toolExe = GetFFMpegExePath();

				if (!File.Exists(toolExe))
					throw new FileNotFoundException("Cannot find ffmpeg tool: " + toolExe);
				
				var startInfo = new ProcessStartInfo(toolExe,ffmpegArgs);
#if !NETSTANDARD2_0
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
#endif
				startInfo.CreateNoWindow = true;
				startInfo.UseShellExecute = false;
				startInfo.WorkingDirectory = Path.GetDirectoryName(FFMpegToolPath);
				startInfo.RedirectStandardInput = true; // for sending 'q' to ffmpeg 
				startInfo.RedirectStandardOutput = false;
				startInfo.RedirectStandardError = true;
				InitStartInfo(startInfo);

				if (FFMpegProcess != null)
					throw new InvalidOperationException("FFMpeg process is already started");

				FFMpegProcess = Process.Start(startInfo);
				if (FFMpegProcessPriority!=ProcessPriorityClass.Normal)
					FFMpegProcess.PriorityClass = FFMpegProcessPriority;

				var lastErrorLine = String.Empty;
				FFMpegProcess.ErrorDataReceived += (o, args) => {
					if (args.Data==null)
						return;
					lastErrorLine = args.Data;
					FFMpegLogHandler(args.Data);
				};
				FFMpegProcess.BeginErrorReadLine();

				WaitFFMpegProcessForExit();

				if (FFMpegProcess.ExitCode != 0) {
					throw new FFMpegException(FFMpegProcess.ExitCode, lastErrorLine);
				}

				FFMpegProcess.Close();
				FFMpegProcess = null;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				EnsureFFMpegProcessStopped();
				throw;
			} 
		}

		internal void FFMpegLogHandler(string line) {
			if (LogReceived != null) {
				LogReceived(this, new FFMpegLogEventArgs(line));
			}
		}

		internal void OnConvertProgress(ConvertProgressEventArgs args) {
			if (ConvertProgress != null) {
				ConvertProgress(this, args);
			}
		}

		internal void ConvertMedia(Media input, Media output, ConvertSettings settings) {
			EnsureFFMpegLibs();
#if TRIAL
			License.L.Check();
#endif

			var inputFile = input.Filename;

			// most formats require seekable input
			if (inputFile == null) {
				inputFile = Path.GetTempFileName();
				using (var outputStream = new FileStream(inputFile, FileMode.Create, FileAccess.Write, FileShare.None)) {
					CopyStream(input.DataStream, outputStream, 256 * 1024);
				}
			}

			var outputFile = output.Filename;

			// most formats require seekable output
			if (outputFile == null) {
				outputFile = Path.GetTempFileName();
			}

			//check audio rate
			if ((output.Format == Format.flv || Path.GetExtension(outputFile).ToLower() == ".flv") && settings.AudioSampleRate == null)
				settings.AudioSampleRate = 44100;

			try {
				var toolExe = GetFFMpegExePath();

				if (!File.Exists(toolExe))
					throw new FileNotFoundException("Cannot find ffmpeg tool: " + toolExe);

				var toolArgs = ComposeFFMpegCommandLineArgs(
									inputFile, input.Format,
									outputFile, output.Format, settings);

				var startInfo = new ProcessStartInfo(toolExe, toolArgs);
#if !NETSTANDARD2_0
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
#endif
				startInfo.CreateNoWindow = true;
				startInfo.UseShellExecute = false;
				startInfo.WorkingDirectory = Path.GetDirectoryName(FFMpegToolPath);
				startInfo.RedirectStandardInput = true; // we can send 'q' to ffmpeg 
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;
				InitStartInfo(startInfo);

				if (FFMpegProcess != null)
					throw new InvalidOperationException("FFMpeg process is already started");

				FFMpegProcess = Process.Start(startInfo);
				//if (FFMpegProcessPriority!=ProcessPriorityClass.Normal)
				//	FFMpegProcess.PriorityClass = FFMpegProcessPriority;

				var lastErrorLine = String.Empty;

				var ffmpegProgress = new FFMpegProgress(OnConvertProgress, ConvertProgress!=null);
				if (settings != null) { 
					ffmpegProgress.Seek = settings.Seek;
					ffmpegProgress.MaxDuration = settings.MaxDuration;
				}

				FFMpegProcess.ErrorDataReceived += (o, args) => {
					if (args.Data==null)
						return;
					lastErrorLine = args.Data;

					ffmpegProgress.ParseLine(args.Data);

					FFMpegLogHandler(args.Data);
				};
				FFMpegProcess.OutputDataReceived += (o, args) => {
				};

				FFMpegProcess.BeginOutputReadLine();
				FFMpegProcess.BeginErrorReadLine();

				WaitFFMpegProcessForExit();

				if (FFMpegProcess.ExitCode != 0) {
					throw new FFMpegException(FFMpegProcess.ExitCode, lastErrorLine);
				}
				FFMpegProcess.Close();
				FFMpegProcess = null;

				ffmpegProgress.Complete();

				if (output.Filename == null) {
					using (var inputStream = new FileStream(outputFile, FileMode.Open, FileAccess.Read, FileShare.None) ) {
						CopyStream(inputStream, output.DataStream, 256 * 1024);
					}
				}

			} catch (Exception ex) {
				Debug.WriteLine(ex);
				EnsureFFMpegProcessStopped();
				throw;
			} finally {
				if (inputFile!=null && input.Filename == null)
					if (File.Exists(inputFile))
						File.Delete(inputFile);
				if (outputFile!=null && output.Filename == null)
					if (File.Exists(outputFile))
						File.Delete(outputFile);
			}
		}

		private static object globalObj = new object();

#if !LIGHT
		/// <summary>
		/// Extracts ffmpeg binaries (if needed) to the location specified by <see cref="FFMpegConverter.FFMpegToolPath"/>.
		/// </summary>
		/// <remarks><para>If missed ffmpeg is extracted automatically before starting conversion process. 
		/// In some cases it is better to do that explicetily on the application start by calling <see cref="FFMpegConverter.ExtractFFmpeg"/> method.</para>
		/// <para>This method is not available in LT version (without embedded ffmpeg binaries).</para></remarks>
		public void ExtractFFmpeg() {
			EnsureFFMpegLibs();
		}
#endif

		private void EnsureFFMpegLibs() {
#if !LIGHT
			var thisAssembly = Assembly.GetExecutingAssembly();
			var resourcesList = thisAssembly.GetManifestResourceNames();
			var resPrefix = "NReco.VideoConverter.FFMpeg.";

			// workaround for strange issue when GetLastWriteTime throws "The path is not of a legal form"
			var assemblyTimestamp = new DateTime(2014,1,1);
			try {
				assemblyTimestamp = File.GetLastWriteTime(thisAssembly.Location);
			} catch { }

			foreach (var resName in resourcesList) {
				if (!resName.StartsWith(resPrefix))
					continue;
				var res = resName.Substring(resPrefix.Length);
				var targetFileName = Path.Combine(FFMpegToolPath, Path.GetFileNameWithoutExtension(res));

				lock (globalObj) {
					if (File.Exists(targetFileName)) {
						if (File.GetLastWriteTime(targetFileName) > assemblyTimestamp)
							continue;
					}

					var resStream = thisAssembly.GetManifestResourceStream(resName);
					using (var inputStream = new GZipStream(resStream, CompressionMode.Decompress, false)) {
						using (var outputStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
							var buf = new byte[64 * 1024];
							int read;
							while ((read = inputStream.Read(buf, 0, buf.Length)) > 0)
								outputStream.Write(buf, 0, read);
						}
					}
				}
			}
#endif
		}

		/// <summary>
		/// Abort FFMpeg process started by ConvertMedia or ConcatMedia methods
		/// </summary>
		/// <remarks>This method IMMEDIATELY stops FFMpeg by killing the process. Resulting file may be inconsistent.</remarks>
		public void Abort() {
			EnsureFFMpegProcessStopped();
		}

		/// <summary>
		/// Stop FFMpeg process "softly" by sending 'q' command to FFMpeg console. 
		/// This method doesn't stop FFMpeg process immediately and may take some time.
		/// </summary>
		/// <returns>true if 'q' command was sent sucessfully and FFPeg process has exited. If this method returns false FFMpeg process should be stopped with Abort method.</returns>
		public bool Stop() {
			if (FFMpegProcess != null && !FFMpegProcess.HasExited) {
				if (FFMpegProcess.StartInfo.RedirectStandardInput) {
					FFMpegProcess.StandardInput.WriteLine("q\n");
					FFMpegProcess.StandardInput.Close();
					WaitFFMpegProcessForExit();
					return true;
				}
			}
			return false;
		}


	}

	/// <summary>
	/// Provides data for ConvertProgress event
	/// </summary>
	public class ConvertProgressEventArgs : EventArgs {
		
		/// <summary>
		/// Total media stream duration
		/// </summary>
		public TimeSpan TotalDuration { get; private set; }
		
		/// <summary>
		/// Processed media stream duration
		/// </summary>
		public TimeSpan Processed { get; private set; }

		public ConvertProgressEventArgs(TimeSpan processed, TimeSpan totalDuration) {
			TotalDuration = totalDuration;
			Processed = processed;
		}
	}

	/// <summary>
	/// Provides data for log received event
	/// </summary>
	public class FFMpegLogEventArgs : EventArgs {

		/// <summary>
		/// Log line
		/// </summary>
		public string Data { get; private set; }

		public FFMpegLogEventArgs(string logData) {
			Data = logData;
		}
	}

	internal class FFMpegProgress {

		static Regex DurationRegex = new Regex(@"Duration:\s(?<duration>[0-9:.]+)([,]|$)",
			RegexOptions.Compiled|RegexOptions.Singleline|RegexOptions.IgnoreCase);
		static Regex ProgressRegex = new Regex(@"time=(?<progress>[0-9:.]+)\s",
			RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

		internal float? Seek = null;
		internal float? MaxDuration = null;
		Action<ConvertProgressEventArgs> ProgressCallback;
		ConvertProgressEventArgs lastProgressArgs = null;
		bool Enabled = true;
		int progressEventCount = 0;

		internal FFMpegProgress(Action<ConvertProgressEventArgs> progressCallback, bool enabled) {
			ProgressCallback = progressCallback;
			Enabled = enabled;
		}

		internal void Reset() {
			progressEventCount = 0;
			lastProgressArgs = null;
		}

		internal void ParseLine(string line) {
			if (Enabled) {
				var totalDuration = lastProgressArgs!=null ? lastProgressArgs.TotalDuration : TimeSpan.Zero;
				var durationMatch = DurationRegex.Match(line);
				if (durationMatch.Success) {
					TimeSpan inputDuration = TimeSpan.Zero; 
					if (TimeSpan.TryParse(durationMatch.Groups["duration"].Value, out inputDuration)) {
						var totalInputDuration = totalDuration.Add(inputDuration);
						lastProgressArgs = new ConvertProgressEventArgs(TimeSpan.Zero, totalInputDuration);
					}
				}

				var progressMatch = ProgressRegex.Match(line);
				if (progressMatch.Success) {
					TimeSpan progressTime = TimeSpan.Zero;
					if (TimeSpan.TryParse(progressMatch.Groups["progress"].Value, out progressTime)) {

						// correct total duration on first progress event
						if (progressEventCount == 0) {
							totalDuration = CorrectDuration(totalDuration);
						}

						lastProgressArgs = new ConvertProgressEventArgs(
							progressTime, 
							totalDuration!=TimeSpan.Zero ? totalDuration : progressTime);
						ProgressCallback(lastProgressArgs);
						progressEventCount++;

					}
				}
			}
		}

		private TimeSpan CorrectDuration(TimeSpan totalDuration) {
			// correct duration if needed
			if (totalDuration != TimeSpan.Zero) {
				if (Seek.HasValue) {
					var seekTimeSpan = TimeSpan.FromSeconds(Seek.Value);
					totalDuration = totalDuration>seekTimeSpan ? totalDuration.Subtract(seekTimeSpan) : TimeSpan.Zero;
				}
				if (MaxDuration.HasValue) {
					var maxDurationTimeSpan = TimeSpan.FromSeconds(MaxDuration.Value);
					if (totalDuration>maxDurationTimeSpan)
						totalDuration = maxDurationTimeSpan;
				}
			}	
			return totalDuration;
		}

		internal void Complete() {
			// fix issue when full progress is not reflected by FFMpeg log
			if (Enabled && lastProgressArgs != null && lastProgressArgs.Processed < lastProgressArgs.TotalDuration) {
				ProgressCallback(new ConvertProgressEventArgs(lastProgressArgs.TotalDuration, lastProgressArgs.TotalDuration));
			}
		}

	}

}
