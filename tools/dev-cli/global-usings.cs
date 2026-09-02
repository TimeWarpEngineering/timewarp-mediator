#region Purpose
// Global usings for the TimeWarp.Mediator dev CLI
#endregion

global using System;
global using System.IO;
global using System.IO.Compression;
global using System.Linq;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Net;
global using System.Net.Http;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Xml.Linq;

global using TimeWarp.Nuru;
global using static TimeWarp.Nuru.Unit;
global using TimeWarp.Amuru;
global using TimeWarp.Terminal;
global using DevCli;
global using DevCli.Commands;
global using Microsoft.Extensions.DependencyInjection;
