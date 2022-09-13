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
using System.Security;

#if NETSTANDARD2_0
using PwdString = System.String;
#else
using PwdString = System.Security.SecureString;
#endif

namespace NReco.VideoConverter {

	/// <summary>
	/// Represents user credential used when starting FFMpeg process.
	/// </summary>
	public sealed class FFMpegUserCredential {

		/// <summary>
		/// Gets the user name to be used when starting FFMpeg process.
		/// </summary>
		public string UserName { get; private set; }

		/// <summary>
		/// Gets a secure string that contains the user password to use when starting FFMpeg process.
		/// </summary>
		public PwdString Password { get; private set; }

		/// <summary>
		/// Gets a value that identifies the domain to use when starting FFMpeg process. 
		/// </summary>
		public string Domain { get; private set; }

		public FFMpegUserCredential(string userName, PwdString password) {
			UserName = userName;
			Password = password;
		}

		public FFMpegUserCredential(string userName, PwdString password, string domain) : this(userName,password) {
			Domain = domain;
		}

	}
}
