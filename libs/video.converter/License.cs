/*
 *  Copyright 2016-2017 Vitaliy Fedorchenko (nrecosite.com)
 *
 *  Licensed under PivotData Source Code Licence (see LICENSE file).
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS 
 *  OF ANY KIND, either express or implied.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;

namespace NReco.VideoConverter {
	
	public static class License {

		internal readonly static LicenseInternal L = new LicenseInternal();

		public static void SetLicenseKey(string owner, string key) {
		}

		public static LicenseInfo GetLicense() {
			return new LicenseInfo("Custom enterprise license build", null);
		}

		public sealed class LicenseInfo {

			public string Owner { get; private set; }
			public string Key { get; private set; }

			internal LicenseInfo(string owner, string key) {
				Owner = owner;
				Key = key;
			}
		}

		internal sealed class LicenseInternal {

			internal LicenseInternal() {
			}

			internal bool IsLicensed() {
				return true;
			}

			internal void Check() {
			}

		}


	}



}
