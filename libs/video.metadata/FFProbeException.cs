/*
 *  Copyright 2015 Vitaliy Fedorchenko
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

namespace NReco.VideoInfo {

	/// <summary>
	/// The exception that is thrown when FFProbe process retruns non-zero error exit code
	/// </summary>
	public class FFProbeException : Exception {

		/// <summary>
		/// Get FFMpeg process error code
		/// </summary>
		public int ErrorCode { get; private set; }

		public FFProbeException(int errCode, string message)
			: base( String.Format("{0} (exit code: {1})", message, errCode) ) {
				ErrorCode = errCode;
		}

	}
}
