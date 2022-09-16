/*
 *  Copyright 2015-2017 Vitaliy Fedorchenko
 *
 *  Licensed under VideoInfo Source Code Licence (see LICENSE file).
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS 
 *  OF ANY KIND, either express or implied.
 */ 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.IO.Compression;
using System.Xml.XPath;

namespace NReco.VideoInfo
{
	/// <summary>
	/// Provides information about media streams, video or audio files (wrapper for FFProbe command line tool)
	/// </summary>
    public class FFProbe
    {
		/// <summary>
		/// Gets or sets path where FFProbe.exe is extracted
		/// </summary>
		/// <remarks>
		/// By default this property initialized with folder with application assemblies.
		/// For ASP.NET applications it is recommended to use "~/App_Code/".
		/// </remarks>
		public string ToolPath { get; set; }

		/// <summary>
		/// Get or set FFProbe tool executive file name ('ffprobe.exe' by default)
		/// </summary>
		public string FFProbeExeName { get; set; }

		/// <summary>
		/// Get or set custom WkHtmlToImage command line arguments
		/// </summary>
		public string CustomArgs { get; set; }

		/// <summary>
		/// Gets or sets FFProbe process priority (Normal by default)
		/// </summary>
		public ProcessPriorityClass ProcessPriority { get; set; }

		/// <summary>
		/// Gets or sets maximum execution time for running FFProbe process (null is by default = no timeout)
		/// </summary>
		public TimeSpan? ExecutionTimeout { get; set; }

		/// <summary>
		/// Include information about file format.
		/// </summary>
		public bool IncludeFormat { get; set; }

		/// <summary>
		/// Include information about media streams.
		/// </summary>
		public bool IncludeStreams { get; set; }

		/// <summary>
		/// Create new instance of HtmlToPdfConverter
		/// </summary>
		public FFProbe() {
			string rootDir = Directory.GetCurrentDirectory();
	
			ToolPath = rootDir;
			FFProbeExeName = "ffprobe";
			ProcessPriority = ProcessPriorityClass.Normal;

			IncludeFormat = true;
			IncludeStreams = true;
		}

		private static object globalObj = new object();

		private void EnsureFFProbe() {
#if !LT			
			var imgGenAssembly = Assembly.GetExecutingAssembly();
			var resourcesList = imgGenAssembly.GetManifestResourceNames();
			var resPrefix = "NReco.VideoInfo.FFProbe.";
			foreach (var resName in resourcesList) {
				if (!resName.StartsWith(resPrefix))
					continue;
				var res = resName.Substring(resPrefix.Length);
				var targetFileName = Path.Combine(ToolPath, Path.GetFileNameWithoutExtension(res) );

				lock (globalObj) {
					if (File.Exists(targetFileName)) {
						if (File.GetLastWriteTime(targetFileName) > File.GetLastWriteTime(imgGenAssembly.Location))
							continue;
					}
					var resStream = imgGenAssembly.GetManifestResourceStream(resName);
					using (var inputStream = new GZipStream(resStream, CompressionMode.Decompress, false)) {
						using (var outputStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write, FileShare.None)) {
							var buf = new byte[128 * 1024];
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
		/// Returns information about local media file or online stream (URL).
		/// </summary>
		/// <param name="inputFile">local file path or URL</param>
		/// <returns>Structured information about media</returns>
		public MediaInfo GetMediaInfo(string inputFile) {
			return new MediaInfo( GetInfoInternal(inputFile) );
		}

		private XPathDocument GetInfoInternal(string input) { 
			EnsureFFProbe();

			try {
				var toolExe = Path.Combine(ToolPath, FFProbeExeName);
				if (!File.Exists(toolExe))
					throw new FileNotFoundException("Cannot locate FFProbe: " + toolExe);

				var toolArgsSb = new StringBuilder();
				toolArgsSb.Append(" -print_format xml -sexagesimal ");

				if (IncludeFormat)
					toolArgsSb.Append(" -show_format ");
				if (IncludeStreams)
					toolArgsSb.Append(" -show_streams ");

				if (!String.IsNullOrEmpty(CustomArgs))
					toolArgsSb.Append(CustomArgs);
				toolArgsSb.AppendFormat(" \"{0}\" ", input);

				var startInfo = new ProcessStartInfo(toolExe, toolArgsSb.ToString() );
#if !NET_STANDARD				
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
#endif
				startInfo.CreateNoWindow = true;
				startInfo.UseShellExecute = false;
				startInfo.WorkingDirectory = Path.GetDirectoryName(ToolPath);
				startInfo.RedirectStandardInput = false;
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;

				var proc = Process.Start(startInfo);
				if (ProcessPriority!=ProcessPriorityClass.Normal)
					proc.PriorityClass = ProcessPriority;

				var lastErrorLine = String.Empty;
				proc.ErrorDataReceived += (o, args) => {
					if (args.Data == null)
						return;
					lastErrorLine += args.Data+"\n";
				};
				proc.BeginErrorReadLine();

				var output = proc.StandardOutput.ReadToEnd();

				WaitProcessForExit(proc);
				CheckExitCode(proc.ExitCode, lastErrorLine);
				proc.Close();

				return new XPathDocument(new StringReader(output) );

			} catch (Exception ex) {
				throw new Exception(ex.Message, ex);
			}
			
		}


		private void WaitProcessForExit(Process proc) {
			if (ExecutionTimeout.HasValue) {
				if (!proc.WaitForExit((int)ExecutionTimeout.Value.TotalMilliseconds)) {
					EnsureProcessStopped(proc);
					throw new FFProbeException(-2, String.Format("FFProbe process exceeded execution timeout ({0}) and was aborted",ExecutionTimeout) );
				}
			} else {
				proc.WaitForExit();
			}
		}

		private void EnsureProcessStopped(Process proc) {
			if (!proc.HasExited) {
				// cleanup
				try {
					proc.Kill();
					proc.Close();
					proc = null;
				} catch (Exception ex) {
					Debug.WriteLine(ex);
				}
			} else {
				proc.Close();
				proc = null;
			}
		}

		private void CheckExitCode(int exitCode, string lastErrLine) {
			if (exitCode != 0) {
				throw new FFProbeException(exitCode, lastErrLine);
			}
		}

    }

}
