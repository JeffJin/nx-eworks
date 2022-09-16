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
using System.Text;

namespace NReco.VideoConverter {

	/// <summary>
	/// Media conversion setting
	/// </summary>
	/// <inherit>NReco.VideoConverter.OutputSettings</inherit>
	public class ConvertSettings : OutputSettings {

		/// <summary>
		/// Add silent audio stream to output
		/// </summary>
		public bool AppendSilentAudioStream = false;

		/// <summary>
		/// Seek to position (in seconds) before converting
		/// </summary>
		public float? Seek = null;

		/// <summary>
		/// Extra custom FFMpeg parameters for 'input'
		/// </summary>
		/// <remarks>
		/// FFMpeg command line arguments inserted before input file parameter (-i)
		/// </remarks>
		public string CustomInputArgs = null;

	}
}
