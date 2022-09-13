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
using System.Xml.XPath;

namespace NReco.VideoInfo {
	
	/// <summary>
	/// Represents information about media file or stream.
	/// </summary>
	public class MediaInfo {

		/// <summary>
		/// Media container format identifier.
		/// </summary>
		public string FormatName {
			get { return GetAttrValue("/ffprobe/format/@format_name"); }
		}

		/// <summary>
		/// Human-readable container format name.
		/// </summary>
		public string FormatLongName {
			get { return GetAttrValue("/ffprobe/format/@format_long_name"); }
		}

		/// <summary>
		/// List of media container tags.
		/// </summary>
		public KeyValuePair<string, string>[] FormatTags {
			get {
				return _FormatTags ?? (_FormatTags = GetTags("/ffprobe/format/tag") );
			}
		}
		KeyValuePair<string, string>[] _FormatTags = null;

		/// <summary>
		/// List of media streams.
		/// </summary>
		public StreamInfo[] Streams {
			get {
				return _Streams ?? (_Streams = GetStreams() );
			}
		}
		StreamInfo[] _Streams = null;

		/// <summary>
		/// Total duration of the media.
		/// </summary>
		public TimeSpan Duration {
			get {
				var duration = GetAttrValue("/ffprobe/format/@duration");
				if (!String.IsNullOrEmpty(duration)) {
					TimeSpan res;
					if (TimeSpan.TryParse(duration, out res))
						return res;
				}
				return TimeSpan.Zero;
			}
		}

		/// <summary>
		/// FFProble XML result.
		/// </summary>
		public XPathDocument Result { get; private set; }

		public MediaInfo(XPathDocument ffProbeResult) {
			Result = ffProbeResult;
		}

		/// <summary>
		/// Returns attribute value from FFProbe XML result.
		/// </summary>
		/// <param name="xpath">XPath selector</param>
		/// <returns>attribute value or null</returns>
		public string GetAttrValue(string xpath) {
			var nav = Result.CreateNavigator();
			var attrNav = nav.SelectSingleNode(xpath);
			if (attrNav==null)
				return null;
			return attrNav.Value;
		}

		KeyValuePair<string, string>[] GetTags(string xpath) {
			var nav = Result.CreateNavigator();
			var res = new List<KeyValuePair<string,string>>();
			var iter = nav.Select(xpath);
			while (iter.MoveNext()) {
				res.Add( new KeyValuePair<string,string>( 
					iter.Current.GetAttribute("key", String.Empty),
					iter.Current.GetAttribute("value", String.Empty) ) );
			}
			
			return res.ToArray();
		}

		StreamInfo[] GetStreams() {
			var res = new List<StreamInfo>();
			var nav = Result.CreateNavigator();
			var iter = nav.Select("/ffprobe/streams/stream/@index");
			while (iter.MoveNext()) {
				res.Add( new StreamInfo(this, iter.Current.Value ) );
			}
			return res.ToArray();
		}


		/// <summary>
		/// Represents information about stream.
		/// </summary>
		public class StreamInfo {
			MediaInfo Info;

			/// <summary>
			/// Stream index
			/// </summary>
			public string Index { get; private set; }

			/// <summary>
			/// Codec name identifier
			/// </summary>
			public string CodecName {
				get { return Info.GetAttrValue(XPathPrefix+"/@codec_name"); }
			}

			/// <summary>
			/// Human-readable codec name.
			/// </summary>
			public string CodecLongName {
				get { return Info.GetAttrValue(XPathPrefix+"/@codec_long_name"); }
			}

			/// <summary>
			/// Codec type (video, audio).
			/// </summary>
			public string CodecType {
				get { return Info.GetAttrValue(XPathPrefix+"/@codec_type"); }
			}

			/// <summary>
			/// Video stream pixel format (if applicable).
			/// </summary>
			/// <remarks>Null is returned if pixel format is not available.</remarks>
			public string PixelFormat {
				get { return Info.GetAttrValue(XPathPrefix+"/@pix_fmt"); }
			}

			/// <summary>
			/// Video frame width (if applicable).
			/// </summary>
			public int Width {
				get {
					var width = Info.GetAttrValue(XPathPrefix+"/@width");
					return ParseInt(width);
				}
			}

			/// <summary>
			/// Video frame height (if applicable)
			/// </summary>
			public int Height {
				get {
					var height = Info.GetAttrValue(XPathPrefix+"/@height");
					return ParseInt(height);
				}
			}

			/// <summary>
			/// Video frame rate per second (if applicable).
			/// </summary>
			public float FrameRate {
				get {
					var fps = Info.GetAttrValue(XPathPrefix+"/@r_frame_rate");
					if (!String.IsNullOrEmpty(fps)) {
						var parts = fps.Split('/');
						if (parts.Length == 2) {
							var p1 = ParseInt(parts[0]);
							var p2 = ParseInt(parts[1]);
							if (p1>0 && p2>0)
								return ((float)p1)/p2;
						}
					}
					return -1;
				}
			}

			int ParseInt(string s) {
				if (!String.IsNullOrEmpty(s)) {
					int i;
					if (Int32.TryParse(s, out i)) {
						return i;
					}
				}
				return -1;
			}

			public KeyValuePair<string, string>[] Tags {
				get {
					return _Tags ?? (_Tags = Info.GetTags(XPathPrefix+"/tag") );
				}
			}
			KeyValuePair<string, string>[] _Tags = null;

			internal StreamInfo(MediaInfo info, string index) {
				Info = info;
				Index = index;
			}

			string XPathPrefix { get { return "/ffprobe/streams/stream[@index=\""+Index+"\"]"; } }

		}
	}

}
