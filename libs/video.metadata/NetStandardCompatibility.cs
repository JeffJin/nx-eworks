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
using System.Reflection;
using System.Collections.Generic;
using System.Text;

namespace NReco.VideoInfo {
	
	internal static class NetStandardCompatibility {
		internal static void Close(this System.IO.StreamWriter wr) {
			wr.Dispose();
		} 
		internal static void Close(this System.IO.Stream stream) {
			stream.Dispose();
		} 
		internal static void Close(this System.Diagnostics.Process p) {
			p.Dispose();
		} 
	}

}
