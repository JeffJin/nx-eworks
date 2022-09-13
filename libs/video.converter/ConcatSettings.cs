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
	/// Media concatenation setting
	/// </summary>
	/// <inherit>NReco.VideoConverter.OutputSettings</inherit>
	public class ConcatSettings : OutputSettings {

		/// <summary>
		/// Determine whether audio stream
		/// </summary>
		public bool ConcatVideoStream = true;

		/// <summary>
		/// Seek to position (in seconds) before converting
		/// </summary>
		public bool ConcatAudioStream = true;

	}
}
