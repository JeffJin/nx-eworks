/*
 *  Copyright 2013-2016 Vitaliy Fedorchenko
 *
 *  Licensed under VideoConverter Source Code Licence (see LICENSE file).
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS 
 *  OF ANY KIND, either express or implied.
 */ 

using System;
using System.Collections.Generic;
using System.Text;

namespace NReco.VideoConverter {

	/// <summary>
	/// The exception that is thrown when FFMpeg process retruns non-zero error exit code
	/// </summary>
	public class FFMpegInput {

		/// <summary>
		/// FFMpeg input (filename, URL or demuxer parameter)
		/// </summary>
		public string Input { get; set; }

		/// <summary>
		/// Input media stream format (if null ffmpeg tries to automatically detect format).
		/// </summary>
		public string Format { get; set; }

		/// <summary>
		/// Extra custom FFMpeg parameters for this input.
		/// </summary>
		/// <remarks>
		/// These FFMpeg command line arguments inserted before input specifier (-i).
		/// </remarks>
		public string CustomInputArgs { get; set; }

		public FFMpegInput(string input)
			: this( input, null ) {
		}

		public FFMpegInput(string input, string format) {
			Input = input;
			Format = format;
		}

	}
}
