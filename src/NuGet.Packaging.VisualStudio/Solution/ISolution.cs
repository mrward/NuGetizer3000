﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Packaging.VisualStudio
{
	interface ISolution
	{
		string Path { get; }

		IProject SelectedProject { get; }
	}
}
