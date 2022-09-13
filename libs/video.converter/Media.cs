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
using System.IO;

namespace NReco.VideoConverter {
	
	internal class Media {
		public string Filename { get; set; }
		public string Format { get; set; }
		public Stream DataStream { get; set; } 
	}
}
