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
	
	public class OutputSettings {
		/// <summary>
		/// Explicit sample rate for audio stream. Usual rates are: 44100, 22050, 11025
		/// </summary>
		public int? AudioSampleRate = null;

		/// <summary>
		/// Audio codec (complete list of audio codecs: ffmpeg -codecs)
		/// </summary>
		public string AudioCodec = null;

		/// <summary>
		/// Explicit video rate for video stream. Usual rates are: 30, 25
		/// </summary>
		public int? VideoFrameRate = null;

		/// <summary>
		/// Number of video frames to record
		/// </summary>
		public int? VideoFrameCount = null;

		/// <summary>
		/// Video frame size (common video sizes are listed in VideoSizes
		/// </summary>
		public string VideoFrameSize = null;

		/// <summary>
		/// Video codec (complete list of video codecs: ffmpeg -codecs)
		/// </summary>
		public string VideoCodec = null;

		/// <summary>
		/// Get or set max duration (in seconds)
		/// </summary>
		public float? MaxDuration = null;

		/// <summary>
		/// Extra custom FFMpeg parameters for 'output'
		/// </summary>
		/// <remarks>
		/// FFMpeg command line arguments inserted after input file parameter (-i) but before output file
		/// </remarks>
		public string CustomOutputArgs = null;

		public void SetVideoFrameSize(int width, int height) {
			VideoFrameSize = String.Format("{0}x{1}", width, height);
		}

		internal void CopyTo(OutputSettings outputSettings) {
			outputSettings.AudioSampleRate = AudioSampleRate;
			outputSettings.AudioCodec = AudioCodec;
			outputSettings.VideoFrameRate = VideoFrameRate;
			outputSettings.VideoFrameCount = VideoFrameCount;
			outputSettings.VideoFrameSize = VideoFrameSize;
			outputSettings.VideoCodec = VideoCodec;
			outputSettings.MaxDuration = MaxDuration;
			outputSettings.CustomOutputArgs = CustomOutputArgs;
		}
	}
}
