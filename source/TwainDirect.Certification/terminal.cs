﻿///////////////////////////////////////////////////////////////////////////////////////
//
//  TwainDirect.Certification.Program
//
//  Our entry point.
//
///////////////////////////////////////////////////////////////////////////////////////
//  Author          Date            Comment
//  M.McLaughlin    01-Jun-2017     Initial Release
///////////////////////////////////////////////////////////////////////////////////////
//  Copyright (C) 2014-2017 Kodak Alaris Inc.
//
//  Permission is hereby granted, free of charge, to any person obtaining a
//  copy of this software and associated documentation files (the "Software"),
//  to deal in the Software without restriction, including without limitation
//  the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  and/or sell copies of the Software, and to permit persons to whom the
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
//  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
//  DEALINGS IN THE SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////////

// Helpers...
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using TwainDirect.Support;

namespace TwainDirect.Certification
{
    /// <summary>
    /// The certification object that we'll use to test and exercise functions
    /// for TWAIN Direct.
    /// </summary>
    class Terminal
    {
        // Public Methods
        #region Public Methods

        /// <summary>
        /// Initialize stuff...
        /// </summary>
        public Terminal()
        {
            // Make sure we have a console...
            Interpreter.CreateConsole();

            // Init stuff...
            m_blSilent = false;
            m_adnssddeviceinfoSnapshot = null;
            m_dnssddeviceinfoSelected = null;
            m_twainlocalscanner = null;
            m_lkeyvalue = new List<KeyValue>();
            m_transactionLast = null;
            m_lcallstack = new List<CallStack>();

            // Set up the base stack with the program arguments, we know
            // this is the base stack for two reasons: first, it has no
            // script, and second, it's first... :)
            CallStack callstack = default(CallStack);
            callstack.functionarguments.aszCmd = Config.GetCommandLine();
            m_lcallstack.Add(callstack);

            // Create the mdns monitor, and start it...
            m_dnssd = new Dnssd(Dnssd.Reason.Monitor);
            m_dnssd.MonitorStart(null, IntPtr.Zero);

            // Build our command table...
            m_ldispatchtable = new List<Interpreter.DispatchTable>();

            // Discovery and Selection...
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdHelp,                         new string[] { "help", "?" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdList,                         new string[] { "list" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdQuit,                         new string[] { "ex", "exit", "q", "quit" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdSelect,                       new string[] { "select" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdSleep,                        new string[] { "sleep" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdStatus,                       new string[] { "status" }));

            // Api commands...
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiClosesession,              new string[] { "close", "closesession", "closeSession" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiCreatesession,             new string[] { "create", "createsession", "createSession" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiGetsession,                new string[] { "get", "getsession", "getSession" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiInfoex,                    new string[] { "info", "infoex" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiReadimageblockmetadata,    new string[] { "readimageblockmetadata", "readImageBlockMetadata" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiReadimageblock,            new string[] { "readimageblock", "readImageBlock" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiReleaseimageblocks,        new string[] { "release", "releaseimageblocks", "releaseImageBlocks" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiSendtask,                  new string[] { "send", "sendtask", "sendTask" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiStartcapturing,            new string[] { "start", "startcapturing", "startCapturing" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiStopcapturing,             new string[] { "stop", "stopcapturing", "stopCapturing" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdApiWaitforevents,             new string[] { "wait", "waitforevents", "waitForEvents" }));

            // Scripting...
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdCall,                         new string[] { "call" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdEcho,                         new string[] { "echo" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdEchopassfail,                 new string[] { "echopassfail" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdGoto,                         new string[] { "goto" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdIf,                           new string[] { "if" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdReturn,                       new string[] { "return" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdRun,                          new string[] { "run" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdRunv,                         new string[] { "runv" }));
            m_ldispatchtable.Add(new Interpreter.DispatchTable(CmdSet,                          new string[] { "set" }));

            // Say hi...
            Assembly assembly = typeof(Terminal).Assembly;
            AssemblyName assemblyname = assembly.GetName();
            Version version = assemblyname.Version;
            DateTime datetime = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.MinorRevision * 2);

            Display("TWAIN Direct Certification v" + version.Major + "." + version.Minor + " " + datetime.ToShortDateString() + " " + ((IntPtr.Size == 4) ? " (32-bit)" : " (64-bit)"));
            Display("Enter \"help\" for more info.");
        }

        /// <summary>
        /// Run the certification tool...
        /// </summary>
        public void Run()
        {
            string szPrompt = "tdc";
            Interpreter interpreter = new Interpreter(szPrompt + ">>> ");

            // Run until told to stop...
            while (true)
            {
                int iCmd;
                bool blDone;
                string szCmd;
                string[] aszCmd;

                // Prompt...
                szCmd = interpreter.Prompt();

                // Tokenize...
                aszCmd = interpreter.Tokenize(szCmd);

                // Expansion...
                for (iCmd = 0; iCmd < aszCmd.Length; iCmd++)
                {
                    // Use the value as a JSON key to get data from the response data...
                    string szValue = aszCmd[iCmd];
                    if (szValue.StartsWith("rj:"))
                    {
                        if (m_transactionLast != null)
                        {
                            string szResponseData = m_transactionLast.GetResponseData();
                            if (!string.IsNullOrEmpty(szResponseData))
                            {
                                bool blSuccess;
                                long lJsonErrorIndex;
                                JsonLookup jsonlookup = new JsonLookup();
                                blSuccess = jsonlookup.Load(szResponseData, out lJsonErrorIndex);
                                if (blSuccess)
                                {
                                    aszCmd[iCmd] = jsonlookup.Get(szValue.Substring(3));
                                }
                            }
                        }
                    }

                    // Use value as a GET key to get a value, we don't allow a null in this
                    // case, it has to be an empty string...
                    else if (szValue.StartsWith("get:"))
                    {
                        if (m_lkeyvalue.Count == 0)
                        {
                            aszCmd[iCmd] = "";
                        }
                        else
                        {
                            bool blFound = false;
                            string szKey = szValue.Substring(4);
                            foreach (KeyValue keyvalue in m_lkeyvalue)
                            {
                                if (keyvalue.szKey == szKey)
                                {
                                    aszCmd[iCmd] = (keyvalue.szValue == null) ? "" : keyvalue.szValue;
                                    blFound = true;
                                    break;
                                }
                            }
                            if (!blFound)
                            {
                                aszCmd[iCmd] = "";

                            }
                        }
                    }

                    // Get data from the top of the call stack...
                    else if (szValue.StartsWith("arg:"))
                    {
                        if ((m_lcallstack == null) || (m_lcallstack.Count == 0))
                        {
                            aszCmd[iCmd] = "";
                        }
                        else
                        {
                            int iIndex;
                            if (int.TryParse(szValue.Substring(4), out iIndex))
                            {
                                CallStack callstack = m_lcallstack[m_lcallstack.Count - 1];
                                if ((callstack.functionarguments.aszCmd != null) && (iIndex >= 0) && ((iIndex + 1) < callstack.functionarguments.aszCmd.Length))
                                {
                                    aszCmd[iCmd] = callstack.functionarguments.aszCmd[iIndex + 1];
                                }
                                else
                                {
                                    aszCmd[iCmd] = "";
                                }
                            }
                        }
                    }
                }

                // Dispatch...
                Interpreter.FunctionArguments functionarguments = default(Interpreter.FunctionArguments);
                functionarguments.aszCmd = aszCmd;
                functionarguments.transaction = m_transactionLast;
                blDone = interpreter.Dispatch(ref functionarguments, m_ldispatchtable);
                if (blDone)
                {
                    return;
                }
                m_transactionLast = functionarguments.transaction;

                // Update the prompt with state information...
                if (m_twainlocalscanner == null)
                {
                    interpreter.SetPrompt(szPrompt + ">>> ");
                }
                else
                {
                    switch (m_twainlocalscanner.GetState())
                    {
                        default: interpreter.SetPrompt(szPrompt + "." + m_twainlocalscanner.GetState() + ">>> "); break;
                        case "noSession": interpreter.SetPrompt(szPrompt + ">>> "); break;
                        case "ready": interpreter.SetPrompt(szPrompt + ".rdy>>> "); break;
                        case "capturing": interpreter.SetPrompt(szPrompt + ".cap>>> "); break;
                        case "draining": interpreter.SetPrompt(szPrompt + ".drn>>> "); break;
                        case "closed": interpreter.SetPrompt(szPrompt + ".cls>>> "); break;
                    }
                }
            }
        }

        #endregion


        // Private Methods (api)
        #region Private Methods (api)

        /// <summary>
        /// Close a session...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiClosesession(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerCloseSession(ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Create a session...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiCreatesession(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerCreateSession(m_dnssddeviceinfoSelected, ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Get the current session object
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiGetsession(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerGetSession(ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Send an infoex command to the selected scanner...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiInfoex(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientInfo(m_dnssddeviceinfoSelected, ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Read an image data block's metadata and thumbnail...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiReadimageblockmetadata(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;
            long lImageBlock;
            bool blGetThumbnail;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }
            if (a_functionarguments.aszCmd.Length < 3)
            {
                Display("please specify image block to read and thumbnail flag...");
                return (false);
            }

            // Get the image block number...
            if (!long.TryParse(a_functionarguments.aszCmd[1], out lImageBlock))
            {
                Display("image block must be a number...");
                return (false);
            }
            if (!bool.TryParse(a_functionarguments.aszCmd[2], out blGetThumbnail))
            {
                Display("thumbnail flag must be true or false...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerReadImageBlockMetadata(lImageBlock, blGetThumbnail, null, ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Read an image data block and it's metadata...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiReadimageblock(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;
            long lImageBlock;
            bool blGetMetadataWithImage;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }
            if (a_functionarguments.aszCmd.Length < 3)
            {
                Display("please specify image block to read and thumbnail flag...");
                return (false);
            }

            // Get the image block number...
            if (!long.TryParse(a_functionarguments.aszCmd[1], out lImageBlock))
            {
                Display("image block must be a number...");
                return (false);
            }
            if (!bool.TryParse(a_functionarguments.aszCmd[2], out blGetMetadataWithImage))
            {
                Display("getmetdata flag must be true or false...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerReadImageBlock(lImageBlock, blGetMetadataWithImage, null, ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Release or or more image blocks, or all image blocks...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiReleaseimageblocks(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;
            long lFirstImageBlock;
            long lLastImageBlock;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }
            if (a_functionarguments.aszCmd.Length < 3)
            {
                Display("please specify the first and last image block to release...");
                return (false);
            }

            // Get the values...
            if (!long.TryParse(a_functionarguments.aszCmd[1], out lFirstImageBlock))
            {
                Display("first image block must be a number...");
                return (false);
            }
            if (!long.TryParse(a_functionarguments.aszCmd[2], out lLastImageBlock))
            {
                Display("last image block must be a number...");
                return (false);
            }

            // Loop so we can handle the release-all scenerio...
            while (true)
            {
                // Make the call...
                apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
                m_twainlocalscanner.ClientScannerReleaseImageBlocks(lFirstImageBlock, lLastImageBlock, ref apicmd);

                // Squirrel away the transaction...
                a_functionarguments.transaction = apicmd.GetTransaction();

                // Scoot...
                if ((lFirstImageBlock != 1) || (lLastImageBlock != int.MaxValue))
                {
                    break;
                }

                // Otherwise, we'll only scoot if we're out of images, we
                // must be in a draining state for this to be allowed...
                if (apicmd.GetSessionState() != "draining")
                {
                    break;
                }

                // If the flag says we're done, then we're done...
                if (apicmd.GetImageBlocksDrained())
                {
                    break;
                }

                // Wait a little before beating up the scanner with another attempt...
                Thread.Sleep(1000);
            }

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Send task...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiSendtask(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;
            string szTask;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }

            // Must supply a task...
            if ((a_functionarguments.aszCmd.Length < 2) || (a_functionarguments.aszCmd[1] == null))
            {
                Display("must supply a task...");
                return (false);
            }

            // Is the argument a file?
            if (File.Exists(a_functionarguments.aszCmd[1]))
            {
                try
                {
                    szTask = File.ReadAllText(a_functionarguments.aszCmd[1]);
                }
                catch (Exception exception)
                {
                    Display("failed to open file...<" + a_functionarguments.aszCmd[1] + "> - " + exception.Message);
                    return (false);
                }
            }
            else
            {
                szTask = a_functionarguments.aszCmd[1];
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerSendTask(szTask, ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Start capturing...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiStartcapturing(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerStartCapturing(ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Stop capturing...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiStopcapturing(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerStopCapturing(ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        /// <summary>
        /// Wait for events, like changes to the session object...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdApiWaitforevents(ref Interpreter.FunctionArguments a_functionarguments)
        {
            ApiCmd apicmd;

            // Validate...
            if ((m_dnssddeviceinfoSelected == null) || (m_twainlocalscanner == null))
            {
                Display("must first select a scanner...");
                return (false);
            }

            // Make the call...
            apicmd = new ApiCmd(m_dnssddeviceinfoSelected);
            m_twainlocalscanner.ClientScannerWaitForEvents(ref apicmd);

            // Squirrel away the transaction...
            a_functionarguments.transaction = apicmd.GetTransaction();

            // Display what we send...
            DisplayApicmd(apicmd);

            // All done...
            return (false);
        }

        #endregion


        // Private Methods (commands)
        #region Private Methods (commands)

        /// <summary>
        /// Call a function...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdCall(ref Interpreter.FunctionArguments a_functionarguments)
        {
            int iLine;
            string szLabel;

            // Validate...
            if (    (a_functionarguments.aszScript == null)
                ||  (a_functionarguments.aszScript.Length < 2)
                ||  (a_functionarguments.aszScript[0] == null)
                ||  (a_functionarguments.aszCmd == null)
                ||  (a_functionarguments.aszCmd.Length < 2)
                ||  (a_functionarguments.aszCmd[1] == null))
            {
                return (false);
            }

            // Search for a match...
            szLabel = ":" + a_functionarguments.aszCmd[1];
            for (iLine = 0; iLine < a_functionarguments.aszScript.Length; iLine++)
            {
                if (a_functionarguments.aszScript[iLine].Trim() == szLabel)
                {
                    // We need this to go to the function...
                    a_functionarguments.blGotoLabel = true;
                    a_functionarguments.iLabelLine = iLine;

                    // We need this to get back...
                    CallStack callstack = default(CallStack);
                    callstack.functionarguments = a_functionarguments;
                    m_lcallstack.Add(callstack);
                    return (false);
                }
            }

            // Ugh...
            Display("function label not found: <" + szLabel + ">");
            return (false);
        }

        /// <summary>
        /// Echo text...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdEcho(ref Interpreter.FunctionArguments a_functionarguments)
        {
            int ii;
            string szLine = "";

            // No data...
            if ((a_functionarguments.aszCmd == null) || (a_functionarguments.aszCmd.Length < 2) || (a_functionarguments.aszCmd[0] == null))
            {
                Display("", true);
                return (false);
            }

            // Turn it into a line, and spit it out...
            for (ii = 1; ii < a_functionarguments.aszCmd.Length; ii++)
            {
                szLine += ((szLine == "") ? "" : " ") + a_functionarguments.aszCmd[ii];
            }
            Display(szLine, true);

            // All done...
            return (false);
        }

        /// <summary>
        /// Display a pass/fail message...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdEchopassfail(ref Interpreter.FunctionArguments a_functionarguments)
        {
            string szLine;
            string szDots = "..........................................................................................................";

            // No data...
            if ((a_functionarguments.aszCmd == null) || (a_functionarguments.aszCmd.Length < 3) || (a_functionarguments.aszCmd[0] == null))
            {
                Display("echopassfail needs two arguments...", true);
                return (false);
            }

            // Build the string...
            szLine = a_functionarguments.aszCmd[1];
            if ((szDots.Length - szLine.Length) > 0)
            {
                szLine += szDots.Substring(0, szDots.Length - szLine.Length);
            }
            else
            {
                szLine += "...";
            }
            szLine += a_functionarguments.aszCmd[2];

            // Spit it out...
            Display(szLine, true);

            // All done...
            return (false);
        }

        /// <summary>
        /// Goto the user...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdGoto(ref Interpreter.FunctionArguments a_functionarguments)
        {
            int iLine;
            string szLabel;

            // Validate...
            if (    (a_functionarguments.aszScript == null)
                ||  (a_functionarguments.aszScript.Length < 2)
                ||  (a_functionarguments.aszScript[0] == null)
                ||  (a_functionarguments.aszCmd == null)
                ||  (a_functionarguments.aszCmd.Length < 2)
                ||  (a_functionarguments.aszCmd[1] == null))
            {
                return (false);
            }

            // Search for a match...
            szLabel = ":" + a_functionarguments.aszCmd[1];
            for (iLine = 0; iLine < a_functionarguments.aszScript.Length; iLine++)
            {
                if (a_functionarguments.aszScript[iLine].Trim() == szLabel)
                {
                    a_functionarguments.blGotoLabel = true;
                    a_functionarguments.iLabelLine = iLine;
                    return (false);
                }
            }

            // Ugh...
            Display("goto label not found: <" + szLabel + ">");
            return (false);
        }

        /// <summary>
        /// Help the user...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdHelp(ref Interpreter.FunctionArguments a_functionarguments)
        {
            string szCommand;

            // Summary...
            if ((a_functionarguments.aszCmd == null) || (a_functionarguments.aszCmd.Length < 2) || (a_functionarguments.aszCmd[1] == null))
            {
                Display("Discovery and Selection");
                Display("help.........................................this text");
                Display("list.........................................list scanners");
                Display("quit.........................................exit the program");
                Display("select {pattern}.............................select a scanner");
                Display("status.......................................status of the program");
                Display("");
                Display("Image Capture APIs (in order of use)");
                Display("infoex.......................................get information about the scanner");
                Display("createSession................................create a new session");
                Display("getSession...................................show the current session object");
                Display("waitForEvents................................wait for events, like session object changes");
                Display("sendTask {task|file}.........................send task");
                Display("startCapturing...............................start capturing new images");
                Display("readImageBlockMetadata {block} {thumbnail}...read metadata for a block");
                Display("readImageBlock {block} {metadata}............read image data block");
                Display("releaseImageBlocks {first} {last}............release images blocks in the scanner");
                Display("stopCapturing................................stop capturing new images");
                Display("closeSession.................................close the current session");
                Display("");
                Display("Scripting");
                Display("call {label}.................................call function");
                Display("echo [text]..................................echo text");
                Display("if {item1} {operator} {item2} goto {label}...if statement");
                Display("return [status]..............................return from call function");
                Display("run [script].................................run a script");
                Display("runv [script]................................run a script verbosely");
                Display("set [key [value]]............................show, set, or delete keys");
                return (false);
            }

            // Get the command...
            szCommand = a_functionarguments.aszCmd[1].ToLower();

            // Discovery and Selection
            #region Discovery and Selection

            // Help...
            if ((szCommand == "help"))
            {
                Display("HELP [COMMAND]");
                Display("Provides assistence with command and their arguments.  It does not");
                Display("go into detail on TWAIN Direct.  Please read the Specifications for");
                Display("more information.");
                Display("");
                Display("Curly brackets {} indicate mandatory arguments to a command.  Square");
                Display("brackets [] indicate optional arguments.");
                return (false);
            }

            // List...
            if ((szCommand == "list"))
            {
                Display("LIST");
                Display("List the scanners that are advertising themselves.  Note that the");
                Display("same scanner make be seen multiple times, if it's being advertised");
                Display("on more than one network.");
                return (false);
            }

            // Quit...
            if ((szCommand == "quit"))
            {
                Display("QUIT");
                Display("Exit from this program.");
                return (false);
            }

            // Select...
            if ((szCommand == "select"))
            {
                Display("SELECT {PATTERN}");
                Display("Selects one of the scanners shown in the list command, which is");
                Display("the scanner that will be accessed by the API commands.  The pattern");
                Display("must match some or all of the name, the IP address, or the note.");
                return (false);
            }

            // Status...
            if ((szCommand == "status"))
            {
                Display("STATUS");
                Display("General information about the current operation of the program.");
                return (false);
            }

            #endregion

            // Image Capture APIs (in order of use)
            #region Image Capture APIs (in order of use)

            // infoex...
            if ((szCommand == "infoex"))
            {
                Display("INFOEX");
                Display("Issues an infoex command to the scanner that picked out using");
                Display("the SELECT command.  The command must be issued before making");
                Display("a call to CREATESESSION.");
                return (false);
            }

            // createSession...
            if ((szCommand == "createsession"))
            {
                Display("CREATESESSION");
                Display("Creates a session for the scanner picked out using the SELECT");
                Display("command.  To end the session use CLOSESESSION.");
                return (false);
            }

            // getSession...
            if ((szCommand == "getsession"))
            {
                Display("GETSESSION");
                Display("Gets infornation about the current session.");
                return (false);
            }

            // waitForEvents...
            if ((szCommand == "waitforevents"))
            {
                Display("WAITFOREVENTS");
                Display("TWAIN Direct is event driven.  The command creates the event");
                Display("monitor used to detect updates to the session object.  It");
                Display("should be called once after CREATESESSION.");
                return (false);
            }

            // sendTask...
            if ((szCommand == "sendtask"))
            {
                Display("SENDTASK {TASK|FILE}");
                Display("Sends a TWAIN Direct task.  The argument can either be the");
                Display("task itself, or a file containing the task.");
                return (false);
            }

            // startCapturing...
            if ((szCommand == "startcapturing"))
            {
                Display("STARTCAPTURING");
                Display("Start capturing images from the scanner.");
                return (false);
            }

            // readImageBlockMetadata...
            if ((szCommand == "readimageblockmetadata"))
            {
                Display("READIMAGEBLOCKMETADATA {BLOCK} {INCLUDETHUMBNAIL}");
                Display("Reads the metadata for the specified image BLOCK, and");
                Display("optionally includes a thumbnail for that image.");
                return (false);
            }

            // readImageBlock...
            if ((szCommand == "readimageblock"))
            {
                Display("READIMAGEBLOCK {BLOCK} {INCLUDEMETADATA}");
                Display("Reads the image data for the specified image BLOCK, and");
                Display("optionally includes the metadata for that image.");
                return (false);
            }

            // releaseImageBlocks...
            if ((szCommand == "releaseimageblocks"))
            {
                Display("RELEASEIMAGEBLOCKS {FIRST} {LAST}");
                Display("Releases the image blocks from FIRST to LAST inclusive.");
                return (false);
            }

            // stopCapturing...
            if ((szCommand == "stopCapturing"))
            {
                Display("STOPCAPTURING");
                Display("Stop capturing images from the scanner.");
                return (false);
            }

            // closeSession...
            if ((szCommand == "closeSession"))
            {
                Display("CLOSESESSION");
                Display("Close the session, which unlocks the scanner.  The user");
                Display("is responsible for releasing any remaining images.");
                return (false);
            }

            #endregion

            // Scripting
            #region Scripting

            // Call...
            if ((szCommand == "call"))
            {
                Display("CALL {FUNCTION}");
                Display("Call the function.");
                return (false);
            }

            // Echo...
            if ((szCommand == "echo"))
            {
                Display("ECHO [TEXT]");
                Display("Echoes the text.  If there is no text an empty line is echoed.");
                return (false);
            }

            // if...
            if ((szCommand == "if"))
            {
                Display("IF {ITEM1} {OPERATOR} {ITEM2} GOTO {LABEL}");
                Display("If the operator for ITEM1 and ITEM2 is true, then goto the");
                Display("label.  For the best experience get in the habit of putting");
                Display("either single or double quotes around the items.");
                Display("");
                Display("Operators");
                Display("==....values are equal (case sensitive)");
                Display("~~....values are equal (case insensitive)");
                Display("!=....values are not equal (case sensitive)");
                Display("!~....values are not equal (case insensitive)");
                Display("");
                Display("Items");
                Display("Items prefixed with 'rj:' indicate that the item is a JSON");
                Display("key in the last command's response payload.  For instance:");
                Display("  if 'rj:results.success' != 'true' goto FAIL");
                Display("Items prefixed with 'get:' indicate that the item is the");
                Display("result of a prior set command.");
                Display("  if 'get:lastsuccess' != 'true' goto FAIL");
                return (false);
            }

            // Return...
            if ((szCommand == "return"))
            {
                Display("RETURN [STATUS]");
                Display("Return from a call function.");
                return (false);
            }

            // Run...
            if ((szCommand == "run"))
            {
                Display("RUN [SCRIPT]");
                Display("Runs the specified script.  SCRIPT is the full path to the script");
                Display("to be run.  If a SCRIPT is not specified, the scripts in the");
                Display("current folder are listed.");
                return (false);
            }

            // Run verbose...
            if ((szCommand == "runv"))
            {
                Display("RUNV [SCRIPT]");
                Display("Runs the specified script.  SCRIPT is the full path to the script");
                Display("to be run.  If a SCRIPT is not specified, the scripts in the");
                Display("current folder are listed.  The script commands are displayed.");
                return (false);
            }

            // Set...
            if ((szCommand == "set"))
            {
                Display("SET {KEY} {VALUE}");
                Display("Set a key to the specified value.  If a KEY is not specified");
                Display("all of the current keys are listed with their values.");
                Display("");
                Display("Values");
                Display("Values prefixed with 'rj:' indicate that the item is a JSON");
                Display("key in the last command's response payload.  For instance:");
                Display("  set success 'rj:results.success'");
                return (false);
            }

            #endregion

            // Well, this ain't good...
            Display("unrecognized command: " + a_functionarguments.aszCmd[1]);

            // All done...
            return (false);
        }

        /// <summary>
        /// Process an if-statement...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdIf(ref Interpreter.FunctionArguments a_functionarguments)
        {
            bool blDoAction = false;
            string szItem1;
            string szItem2;
            string szOperator;
            string szAction;

            // Validate...
            if ((a_functionarguments.aszCmd == null) || (a_functionarguments.aszCmd.Length < 4) || (a_functionarguments.aszCmd[1] == null))
            {
                Display("badly formed if-statement...");
                return (false);
            }

            // Get all of the stuff...
            szItem1 = a_functionarguments.aszCmd[1];
            szOperator = a_functionarguments.aszCmd[2];
            szItem2 = a_functionarguments.aszCmd[3];
            szAction = a_functionarguments.aszCmd[4];

            // Items must match (case sensitive)...
            if (szOperator == "==")
            {
                if (szItem1 == szItem2)
                {
                    blDoAction = true;
                }
            }

            // Items must match (case insensitive)...
            else if (szOperator == "~~")
            {
                if (szItem1.ToLowerInvariant() == szItem2.ToLowerInvariant())
                {
                    blDoAction = true;
                }
            }

            // Items must not match (case sensitive)...
            else if (szOperator == "!=")
            {
                if (szItem1 != szItem2)
                {
                    blDoAction = true;
                }
            }

            // Items must not match (case insensitive)...
            else if (szOperator == "!~")
            {
                if (szItem1.ToLowerInvariant() != szItem2.ToLowerInvariant())
                {
                    blDoAction = true;
                }
            }

            // Unrecognized operator...
            else
            {
                Display("unrecognized operator: <" + szOperator + ">");
                return (false);
            }

            // We've been told to do the action...
            if (blDoAction)
            {
                // We're doing a goto...
                if (szAction.ToLowerInvariant() == "goto")
                {
                    int iLine;
                    string szLabel;

                    // Validate...
                    if ((a_functionarguments.aszCmd.Length < 5) || string.IsNullOrEmpty(a_functionarguments.aszCmd[4]))
                    {
                        Display("goto label is missing...");
                        return (false);
                    }

                    // Find the label...
                    szLabel = ":" + a_functionarguments.aszCmd[5];
                    for (iLine = 0; iLine < a_functionarguments.aszScript.Length; iLine++)
                    {
                        if (a_functionarguments.aszScript[iLine].Trim() == szLabel)
                        {
                            a_functionarguments.blGotoLabel = true;
                            a_functionarguments.iLabelLine = iLine;
                            return (false);
                        }
                    }

                    // Ugh...
                    Display("goto label not found: <" + szLabel + ">");
                    return (false);
                }

                // We have no idea what we're doing...
                else
                {
                    Display("unrecognized action: <" + szAction + ">");
                    return (false);
                }
            }

            // All done...
            return (false);
        }

        /// <summary>
        /// List scanners, both ones on the LAN and ones that are
        /// available in the cloud (when we get that far)...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdList(ref Interpreter.FunctionArguments a_functionarguments)
        {
            bool blUpdated;

            // Get a snapshot of the TWAIN Local scanners...
            m_adnssddeviceinfoSnapshot = m_dnssd.GetSnapshot(null, out blUpdated);

            // Display TWAIN Local...
            if (!m_blSilent)
            {
                if ((m_adnssddeviceinfoSnapshot == null) || (m_adnssddeviceinfoSnapshot.Length == 0))
                {
                    Display("*** no TWAIN Local scanners ***");
                }
                else
                {
                    foreach (Dnssd.DnssdDeviceInfo dnssddeviceinfo in m_adnssddeviceinfoSnapshot)
                    {
                        Display(dnssddeviceinfo.szLinkLocal + " " + (!string.IsNullOrEmpty(dnssddeviceinfo.szIpv4) ? dnssddeviceinfo.szIpv4 : dnssddeviceinfo.szIpv6) + " " + dnssddeviceinfo.szTxtNote);
                    }
                }
            }

            // All done...
            return (false);
        }

        /// <summary>
        /// Quit...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdQuit(ref Interpreter.FunctionArguments a_functionarguments)
        {
            // Bye-bye...
            return (true);
        }

        /// <summary>
        /// Return from the current function...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdReturn(ref Interpreter.FunctionArguments a_functionarguments)
        {
            CallStack callstack;

            // If we don't have anything on the stack, then scoot...
            if ((m_lcallstack == null) || (m_lcallstack.Count == 0))
            {
                return (false);
            }

            // If this is the base of the stack, then return is a noop...
            if (m_lcallstack.Count == 1)
            {
                return (false);
            }

            // Make a copy of the last item (which we're about to delete)...
            callstack = m_lcallstack[m_lcallstack.Count - 1];

            // Remove the last item...
            m_lcallstack.RemoveAt(m_lcallstack.Count - 1);

            // Set the line we want to jump back to...
            a_functionarguments.blGotoLabel = true;
            a_functionarguments.iLabelLine = callstack.functionarguments.iCurrentLine + 1;

            // Make a note of the return value for "ret:"...
            if ((a_functionarguments.aszCmd != null) && (a_functionarguments.aszCmd.Length > 1))
            {
                callstack = m_lcallstack[m_lcallstack.Count - 1];
                callstack.functionarguments.szReturnValue = a_functionarguments.aszCmd[1];
                m_lcallstack[m_lcallstack.Count - 1] = callstack;
            }

            // All done...
            return (false);
        }

        /// <summary>
        /// With no arguments, list the scripts.  With an argument,
        /// run the specified script.  This one runs silent.
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdRun(ref Interpreter.FunctionArguments a_functionarguments)
        {
            bool blSuccess;
            bool blSilent = m_blSilent;
            m_blSilent = true;
            blSuccess = CmdRunv(ref a_functionarguments);
            m_blSilent = blSilent;
            return (blSuccess);
        }

        /// <summary>
        /// With no arguments, list the scripts.  With an argument,
        /// run the specified script.  The one runs verbose.
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdRunv(ref Interpreter.FunctionArguments a_functionarguments)
        {
            string szPrompt = "tdc>>> ";
            string[] aszScript;
            string szScriptFile;
            int iCallStackCount;
            CallStack callstack;
            Interpreter interpreter;

            // List...
            if ((a_functionarguments.aszCmd == null) || (a_functionarguments.aszCmd.Length < 2) || (a_functionarguments.aszCmd[1] == null))
            {
                // Get the script files...
                string[] aszScriptFiles = Directory.GetFiles(".", "*.tdc");
                if ((aszScriptFiles == null) || (aszScriptFiles.Length == 0))
                {
                    Display("no script files found");
                }

                // List what we found...
                Display("SCRIPT FILES");
                foreach (string sz in aszScriptFiles)
                {
                    Display(sz.Replace(".tdc", ""));
                }

                // All done...
                return (false);
            }

            // Make sure the file exists...
            szScriptFile = a_functionarguments.aszCmd[1];
            if (!File.Exists(szScriptFile))
            {
                szScriptFile = a_functionarguments.aszCmd[1] + ".tdc";
                if (!File.Exists(szScriptFile))
                {
                    Display("script not found");
                    return (false);
                }
            }

            // Read the file...
            try
            {
                aszScript = File.ReadAllLines(szScriptFile);
            }
            catch (Exception exception)
            {
                Display("failed to read script: " + exception.Message);
                return (false);
            }

            // Give ourselves an interpreter...
            interpreter = new Interpreter("");

            // Bump ourself up on the call stack, because we're really
            // working like a call.  At this point we'll be running with
            // at least 2 items on the stack.  If we drop down to 1 item
            // that's a hint that the return command was used to get out
            // of the script...
            callstack = default(CallStack);
            callstack.functionarguments = a_functionarguments;
            callstack.functionarguments.aszScript = aszScript;
            m_lcallstack.Add(callstack);
            iCallStackCount = m_lcallstack.Count;

            // Run each line in the script...
            int iLine = 0;
            while (iLine < aszScript.Length)
            {
                int iCmd;
                bool blDone;
                string szLine;
                string[] aszCmd;

                // Grab our line...
                szLine = aszScript[iLine];

                // Show the command...
                if (!m_blSilent)
                {
                    Display(szPrompt + szLine.Trim());
                }

                // Tokenize...
                aszCmd = interpreter.Tokenize(szLine.Trim());

                // Expansion...
                for (iCmd = 0; iCmd < aszCmd.Length; iCmd++)
                {
                    // Use the value as a JSON key to get data from the response data...
                    string szValue = aszCmd[iCmd];
                    if (szValue.StartsWith("rj:"))
                    {
                        if (m_transactionLast != null)
                        {
                            string szResponseData = m_transactionLast.GetResponseData();
                            if (!string.IsNullOrEmpty(szResponseData))
                            {
                                bool blSuccess;
                                long lJsonErrorIndex;
                                JsonLookup jsonlookup = new JsonLookup();
                                blSuccess = jsonlookup.Load(szResponseData, out lJsonErrorIndex);
                                if (blSuccess)
                                {
                                    aszCmd[iCmd] = jsonlookup.Get(szValue.Substring(3));
                                }
                            }
                        }
                    }

                    // Use value as a GET key to get a value...
                    else if (szValue.StartsWith("get:"))
                    {
                        if (m_lkeyvalue.Count == 0)
                        {
                            aszCmd[iCmd] = "";
                        }
                        else
                        {
                            bool blFound = false;
                            string szKey = szValue.Substring(4);
                            foreach (KeyValue keyvalue in m_lkeyvalue)
                            {
                                if (keyvalue.szKey == szKey)
                                {
                                    aszCmd[iCmd] = (keyvalue.szValue == null) ? "" : keyvalue.szValue;
                                    blFound = true;
                                    break;
                                }
                            }
                            if (!blFound)
                            {
                                aszCmd[iCmd] = "";
                            }
                        }
                    }

                    // Get data from the top of the call stack...
                    else if (szValue.StartsWith("arg:"))
                    {
                        if ((m_lcallstack == null) || (m_lcallstack.Count == 0))
                        {
                            aszCmd[iCmd] = "";
                        }
                        else
                        {
                            int iIndex;
                            if (int.TryParse(szValue.Substring(4), out iIndex))
                            {
                                callstack = m_lcallstack[m_lcallstack.Count - 1];
                                if ((callstack.functionarguments.aszCmd != null) && (iIndex >= 0) && ((iIndex + 1) < callstack.functionarguments.aszCmd.Length))
                                {
                                    aszCmd[iCmd] = callstack.functionarguments.aszCmd[iIndex + 1];
                                }
                                else
                                {
                                    aszCmd[iCmd] = "";
                                }
                            }
                        }
                    }

                    // Get data from the return value...
                    else if (szValue.StartsWith("ret:"))
                    {
                        callstack = m_lcallstack[m_lcallstack.Count - 1];
                        if (callstack.functionarguments.szReturnValue != null)
                        {
                            aszCmd[iCmd] = callstack.functionarguments.szReturnValue;
                        }
                        else
                        {
                            aszCmd[iCmd] = "";
                        }
                    }
                }

                // Dispatch...
                Interpreter.FunctionArguments functionarguments = default(Interpreter.FunctionArguments);
                functionarguments.aszCmd = aszCmd;
                functionarguments.aszScript = aszScript;
                functionarguments.iCurrentLine = iLine;
                functionarguments.transaction = m_transactionLast;
                blDone = interpreter.Dispatch(ref functionarguments, m_ldispatchtable);
                if (blDone)
                {
                    break;
                }
                m_transactionLast = functionarguments.transaction;

                // Handle gotos...
                if (functionarguments.blGotoLabel)
                {
                    iLine = functionarguments.iLabelLine;
                }
                // Otherwise, just increment...
                else
                {
                    iLine += 1;
                }

                // Update the prompt with state information...
                if (m_twainlocalscanner == null)
                {
                    szPrompt = "tdc>>> ";
                }
                else
                {
                    switch (m_twainlocalscanner.GetState())
                    {
                        default: szPrompt = "tdc." + m_twainlocalscanner.GetState() + ">>> "; break;
                        case "noSession": szPrompt = "tdc>>> "; break;
                        case "ready": szPrompt = "tdc.rdy>>> "; break;
                        case "capturing": szPrompt = "tdc.cap>>> "; break;
                        case "draining": szPrompt = "tdc.drn>>> "; break;
                        case "closed": szPrompt = "tdc.cls>>> "; break;
                    }
                }

                // If the count dropped, that's a sign we need to bail...
                if (m_lcallstack.Count < iCallStackCount)
                {
                    break;
                }
            }

            // Pop this item, and pass along the return value...
            if (m_lcallstack.Count > 1)
            {
                string szReturnValue = m_lcallstack[m_lcallstack.Count - 1].functionarguments.szReturnValue;
                if (szReturnValue == null)
                {
                    szReturnValue = "";
                }
                m_lcallstack.RemoveAt(m_lcallstack.Count - 1);
                callstack = m_lcallstack[m_lcallstack.Count - 1];
                callstack.functionarguments.szReturnValue = szReturnValue;
                m_lcallstack[m_lcallstack.Count - 1] = callstack;
            }

            // All done...
            return (false);
        }

        /// <summary>
        /// Select a scanner, do a snapshot, if needed, if no selection
        /// is offered, then pick the first scanner found...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdSelect(ref Interpreter.FunctionArguments a_functionarguments)
        {
            bool blSilent;

            // Clear the last selected scanner...
            m_dnssddeviceinfoSelected = null;
            if (m_twainlocalscanner != null)
            {
                m_twainlocalscanner.Dispose();
                m_twainlocalscanner = null;
            }

            // If we don't have a snapshot, get one...
            if ((m_adnssddeviceinfoSnapshot == null) || (m_adnssddeviceinfoSnapshot.Length == 0))
            {
                blSilent = m_blSilent;
                m_blSilent = true;
                Interpreter.FunctionArguments functionarguments = default(Interpreter.FunctionArguments);
                CmdList(ref functionarguments);
                m_blSilent = blSilent;
            }

            // No joy...
            if ((m_adnssddeviceinfoSnapshot == null) || (m_adnssddeviceinfoSnapshot.Length == 0))
            {
                Display("*** no TWAIN Local scanners ***");
                SetReturnValue("false");
                return (false);
            }

            // We didn't get a selection, so grab the first item...
            if ((a_functionarguments.aszCmd == null) || (a_functionarguments.aszCmd.Length < 2) || string.IsNullOrEmpty(a_functionarguments.aszCmd[1]))
            {
                m_dnssddeviceinfoSelected = m_adnssddeviceinfoSnapshot[0];
                SetReturnValue("true");
                return (false);
            }

            // Look for a match...
            foreach (Dnssd.DnssdDeviceInfo dnssddeviceinfo in m_adnssddeviceinfoSnapshot)
            {
                // Check the name...
                if (!string.IsNullOrEmpty(dnssddeviceinfo.szLinkLocal) && dnssddeviceinfo.szLinkLocal.Contains(a_functionarguments.aszCmd[1]))
                {
                    m_dnssddeviceinfoSelected = dnssddeviceinfo;
                    break;
                }

                // Check the IPv4...
                else if (!string.IsNullOrEmpty(dnssddeviceinfo.szIpv4) && dnssddeviceinfo.szIpv4.Contains(a_functionarguments.aszCmd[1]))
                {
                    m_dnssddeviceinfoSelected = dnssddeviceinfo;
                    break;
                }

                // Check the note...
                else if (!string.IsNullOrEmpty(dnssddeviceinfo.szTxtNote) && dnssddeviceinfo.szTxtNote.Contains(a_functionarguments.aszCmd[1]))
                {
                    m_dnssddeviceinfoSelected = dnssddeviceinfo;
                    break;
                }
            }

            // Report the result...
            if (m_dnssddeviceinfoSelected != null)
            {
                Display(m_dnssddeviceinfoSelected.szLinkLocal + " " + (!string.IsNullOrEmpty(m_dnssddeviceinfoSelected.szIpv4) ? m_dnssddeviceinfoSelected.szIpv4 : m_dnssddeviceinfoSelected.szIpv6) + " " + m_dnssddeviceinfoSelected.szTxtNote);
                m_twainlocalscanner = new TwainLocalScanner(null, 1, null, null, null);
                SetReturnValue("true");
            }
            else
            {
                Display("*** no selection matches ***");
                SetReturnValue("false");
            }

            // All done...
            return (false);
        }

        /// <summary>
        /// Set the return value on the top callstack item...
        /// </summary>
        /// <param name="a_szReturn"></param>
        /// <returns></returns>
        private void SetReturnValue(string a_szReturnValue)
        {
            if (m_lcallstack.Count < 1) return;
            CallStack callstack = m_lcallstack[m_lcallstack.Count - 1];
            callstack.functionarguments.szReturnValue = a_szReturnValue;
            m_lcallstack[m_lcallstack.Count - 1] = callstack;
        }

        /// <summary>
        /// With no arguments, list the keys with their values.  With an argument,
        /// set the specified value.
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdSet(ref Interpreter.FunctionArguments a_functionarguments)
        {
            int iKey;

            // If we don't have any arguments, list what we have...
            if ((a_functionarguments.aszCmd == null) || (a_functionarguments.aszCmd.Length < 2) || (a_functionarguments.aszCmd[1] == null))
            {
                if (m_lkeyvalue.Count == 0)
                {
                    Display("no keys to list...");
                    return (false);
                }

                // Loopy...
                Display("KEY/VALUE PAIRS");
                foreach (KeyValue keyvalue in m_lkeyvalue)
                {
                    Display(keyvalue.szKey + "=" + keyvalue.szValue);
                }

                // All done...
                return (false);
            }

            // Find the value for this key...
            for (iKey = 0; iKey < m_lkeyvalue.Count; iKey++)
            {
                if (m_lkeyvalue[iKey].szKey == a_functionarguments.aszCmd[1])
                {
                    break;
                }
            }

            // If we have no value to set, then delete this item...
            if ((a_functionarguments.aszCmd.Length < 3) || (a_functionarguments.aszCmd[2] == null))
            {
                if (iKey < m_lkeyvalue.Count)
                {
                    m_lkeyvalue.Remove(m_lkeyvalue[iKey]);
                }
                return (false);
            }

            // Create a new keyvalue...
            KeyValue keyvalueNew = new KeyValue();
            keyvalueNew.szKey = a_functionarguments.aszCmd[1];
            keyvalueNew.szValue = a_functionarguments.aszCmd[2];

            // If the key already exists, update it's value...
            if (iKey < m_lkeyvalue.Count)
            {
                m_lkeyvalue[iKey] = keyvalueNew;
                return (false);
            }

            // Otherwise, add it, and sort...
            m_lkeyvalue.Add(keyvalueNew);
            m_lkeyvalue.Sort(SortByKeyAscending);

            // All done...
            return (false);
        }

        /// <summary>
        /// A comparison operator for sorting keys in CmdSet...
        /// </summary>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <returns></returns>
        private int SortByKeyAscending(KeyValue a_keyvalue1, KeyValue a_keyvalue2)
        {

            return (a_keyvalue1.szKey.CompareTo(a_keyvalue2.szKey));
        }

        /// <summary>
        /// Sleep some number of milliseconds...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdSleep(ref Interpreter.FunctionArguments a_functionarguments)
        {
            int iMilliseconds;

            // Get the milliseconds...
            if ((a_functionarguments.aszCmd == null) || (a_functionarguments.aszCmd.Length < 2) || !int.TryParse(a_functionarguments.aszCmd[1], out iMilliseconds))
            {
                iMilliseconds = 0;
            }
            if (iMilliseconds < 0)
            {
                iMilliseconds = 0;
            }

            // Wait...
            Thread.Sleep(iMilliseconds);

            // All done...
            return (false);
        }

        /// <summary>
        /// Status of the program...
        /// </summary>
        /// <param name="a_functionarguments">tokenized command and anything needed</param>
        /// <returns>true to quit</returns>
        private bool CmdStatus(ref Interpreter.FunctionArguments a_functionarguments)
        {
            // Current scanner...
            Display("SELECTED SCANNER");
            if (m_dnssddeviceinfoSelected == null)
            {
                Display("*** no selected scanner ***");
            }
            else
            {
                Display(m_dnssddeviceinfoSelected.szLinkLocal + " " + (!string.IsNullOrEmpty(m_dnssddeviceinfoSelected.szIpv4) ? m_dnssddeviceinfoSelected.szIpv4 : m_dnssddeviceinfoSelected.szIpv6) + " " + m_dnssddeviceinfoSelected.szTxtNote);
            }

            // Current snapshot of scanners...
            Display("");
            Display("LAST SCANNER LIST SNAPSHOT");
            if ((m_adnssddeviceinfoSnapshot == null) || (m_adnssddeviceinfoSnapshot.Length == 0))
            {
                Display("*** no TWAIN Local scanners ***");
            }
            else
            {
                foreach (Dnssd.DnssdDeviceInfo dnssddeviceinfo in m_adnssddeviceinfoSnapshot)
                {
                    Display(dnssddeviceinfo.szLinkLocal + " " + (!string.IsNullOrEmpty(dnssddeviceinfo.szIpv4) ? dnssddeviceinfo.szIpv4 : dnssddeviceinfo.szIpv6) + " " + dnssddeviceinfo.szTxtNote);
                }
            }

            // All done...
            return (false);
        }

        /// <summary>
        /// Display text (if allowed)...
        /// </summary>
        /// <param name="a_szText">the text to display</param>
        private void Display(string a_szText, bool a_blForce = false)
        {
            if (!m_blSilent || a_blForce)
            {
                Console.Out.WriteLine(a_szText);
            }
        }

        /// <summary>
        /// Display information about this apicmd object...
        /// </summary>
        /// <param name="a_apicmd">the object we want to display</param>
        private void DisplayApicmd
        (
            ApiCmd a_apicmd
        )
        {
            // Nope...
            if (m_blSilent)
            {
                return;
            }

            // Do it...
            ApiCmd.Transaction transaction = new ApiCmd.Transaction(a_apicmd);
            List<string> lszTransation = transaction.GetAll();
            if (lszTransation != null)
            {
                foreach (string sz in lszTransation)
                {
                    Display(sz);
                }
            }
        }

        #endregion


        // Private Methods (certification)
        #region Private Methods (certification)

        /// <summary>
        /// Run the TWAIN Certification tests.  
        /// </summary>
        private void TwainDirectCertification()
        {
            int ii;
            int iPass = 0;
            int iFail = 0;
            int iSkip = 0;
            int iTotal = 0;
            bool blSuccess;
            long lJsonErrorIndex;
            long lTaskIndex;
            string szCertificationFolder;
            string[] aszCategories;
            string[] aszTestFiles;
            string szTestData;
            string[] aszTestData;
            JsonLookup jsonlookupTest;
            JsonLookup jsonlookupReply;
            ApiCmd apicmd;

            // Find our cert stuff...
            szCertificationFolder = Path.Combine(Config.Get("writeFolder", ""), "tasks");
            szCertificationFolder = Path.Combine(szCertificationFolder, "certification");

            // Whoops...nothing to work with...
            if (!Directory.Exists(szCertificationFolder))
            {
                Display("Cannot find certification folder:\n" + szCertificationFolder);
                return;
            }

            // Get the categories...
            aszCategories = Directory.GetDirectories(szCertificationFolder);
            if (aszCategories == null)
            {
                Display("Cannot find any certification categories:\n" + szCertificationFolder);
                return;
            }

            // Loop the catagories...
            foreach (string szCategory in aszCategories)
            {
                // Get the tests...
                aszTestFiles = Directory.GetFiles(Path.Combine(szCertificationFolder, szCategory));
                if (aszTestFiles == null)
                {
                    continue;
                }

                // Loop the tests...
                foreach (string szTestFile in aszTestFiles)
                {
                    string szSummary;
                    string szStatus;

                    // Log it...
                    Log.Info("");
                    Log.Info("certification>>> file........................." + szTestFile);

                    // The total...
                    iTotal += 1;

                    // Add a new item to show what we're doing...
                    jsonlookupTest = new JsonLookup();

                    // Init stuff...
                    szSummary = Path.GetFileNameWithoutExtension(szTestFile);
                    szStatus = "skip";

                    // Load the test...
                    szTestData = File.ReadAllText(szTestFile);
                    if (string.IsNullOrEmpty(szTestData))
                    {
                        Log.Info("certification>>> status.......................skip (empty file)");
                        iSkip += 1;
                        continue;
                    }

                    // Split the data...
                    if (!szTestData.Contains("***DATADATADATA***"))
                    {
                        Log.Info("certification>>> status.......................skip (data error)");
                        iSkip += 1;
                        continue;
                    }
                    aszTestData = szTestData.Split(new string[] { "***DATADATADATA***\r\n", "***DATADATADATA***\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (aszTestData.Length != 2)
                    {
                        Log.Info("certification>>> status.......................skip (data error)");
                        iSkip += 1;
                        continue;
                    }

                    // Always start this part with a clean slate...
                    apicmd = new ApiCmd(m_dnssddeviceinfoSelected);

                    // Get our instructions...
                    blSuccess = jsonlookupTest.Load(aszTestData[0], out lJsonErrorIndex);
                    if (!blSuccess)
                    {
                        Log.Info("certification>>> status.......................skip (json error)");
                        iSkip += 1;
                        continue;
                    }

                    // Validate the instructions...
                    if (string.IsNullOrEmpty(jsonlookupTest.Get("category")))
                    {
                        Log.Info("certification>>> status.......................ERROR (missing category)");
                        iSkip += 1;
                        continue;
                    }
                    if (string.IsNullOrEmpty(jsonlookupTest.Get("summary")))
                    {
                        Log.Info("certification>>> status.......................skip (missing summary)");
                        iSkip += 1;
                        continue;
                    }
                    if (string.IsNullOrEmpty(jsonlookupTest.Get("description")))
                    {
                        Log.Info("certification>>> status.......................skip (missing description)");
                        iSkip += 1;
                        continue;
                    }
                    if (string.IsNullOrEmpty(jsonlookupTest.Get("expects")))
                    {
                        Log.Info("certification>>> status.......................skip (missing expects)");
                        iSkip += 1;
                        continue;
                    }

                    // Log what we're doing...
                    Log.Info("certification>>> summary......................" + jsonlookupTest.Get("summary"));
                    Log.Info("certification>>> description.................." + jsonlookupTest.Get("description"));
                    for (ii = 0; ; ii++)
                    {
                        string szExpects = "expects[" + ii + "]";
                        if (string.IsNullOrEmpty(jsonlookupTest.Get(szExpects, false)))
                        {
                            break;
                        }
                        Log.Info("certification>>> " + szExpects + ".success..........." + jsonlookupTest.Get(szExpects + ".success"));
                        if (jsonlookupTest.Get(szExpects + ".success") == "false")
                        {
                            Log.Info("certification>>> " + szExpects + ".code.............." + jsonlookupTest.Get(szExpects + ".code"));
                            if (jsonlookupTest.Get(szExpects + ".code") == "invalidJson")
                            {
                                Log.Info("certification>>> " + szExpects + ".characterOffset..." + jsonlookupTest.Get(szExpects + ".characterOffset"));
                            }
                            if (jsonlookupTest.Get(szExpects + ".code") == "invalidValue")
                            {
                                Log.Info("certification>>> " + szExpects + ".jsonKey..........." + jsonlookupTest.Get(szExpects + ".jsonKey"));
                            }
                        }
                    }

                    // Make sure the last item is showing, and then show it...
                    szSummary = jsonlookupTest.Get("summary");
                    szStatus = "(running)";

                    // Perform the test...
                    blSuccess = m_twainlocalscanner.ClientScannerSendTask(aszTestData[1], ref apicmd);
                    if (!blSuccess)
                    {
                        //mlmtbd Add errror check...
                    }

                    // Figure out the index offset to the task, so that we don't
                    // have to dink with the certification tests if the API is
                    // changed for any reason.  Note that we're assuming that the
                    // API is packed...
                    string szSendCommand = apicmd.GetSendCommand();
                    lTaskIndex = (szSendCommand.IndexOf("\"task\":") + 7);

                    // Check out the reply...
                    string szHttpReplyData = apicmd.HttpResponseData();
                    jsonlookupReply = new JsonLookup();
                    blSuccess = jsonlookupReply.Load(szHttpReplyData, out lJsonErrorIndex);
                    if (!blSuccess)
                    {
                        Log.Info("certification>>> status.......................fail (json error)");
                        szStatus = "fail";
                        iFail += 1;
                        continue;
                    }

                    // Check for a task...
                    szHttpReplyData = jsonlookupReply.Get("results.session.task");
                    if (!string.IsNullOrEmpty(szHttpReplyData))
                    {
                        jsonlookupReply = new JsonLookup();
                        blSuccess = jsonlookupReply.Load(szHttpReplyData, out lJsonErrorIndex);
                        if (!blSuccess)
                        {
                            Log.Info("certification>>> status.......................fail (json error)");
                            szStatus = "fail";
                            iFail += 1;
                            continue;
                        }
                    }

                    // Loopy...
                    for (ii = 0; ; ii++)
                    {
                        // Make sure we have this entry...
                        string szExpects = "expects[" + ii + "]";
                        if (string.IsNullOrEmpty(jsonlookupTest.Get(szExpects, false)))
                        {
                            break;
                        }

                        // We need to bump the total for values of ii > 0, this handles
                        // tasks with multiple actions...
                        if (ii > 0)
                        {
                            iTotal += 1;
                        }

                        // We need the path to the results...
                        string szPath = jsonlookupTest.Get(szExpects + ".path");
                        if (string.IsNullOrEmpty(szPath))
                        {
                            szPath = "";
                        }
                        else
                        {
                            szPath += ".";
                        }

                        // The command is expected to succeed...
                        if (jsonlookupTest.Get(szExpects + ".success") == "true")
                        {
                            // Check success...
                            if (string.IsNullOrEmpty(jsonlookupReply.Get(szPath + "results.success")))
                            {
                                Log.Info("certification>>> status.......................fail (missing " + szPath + "results.success)");
                                szStatus = "fail (missing " + szPath + "results.success)";
                                iFail += 1;
                            }
                            else if (jsonlookupReply.Get(szPath + "results.success") != "true")
                            {
                                Log.Info("certification>>> status.......................fail (expected " + szPath + "results.success to be 'true')");
                                szStatus = "fail (expected " + szPath + "results.success to be 'true')";
                                iFail += 1;
                            }
                            else
                            {
                                Log.Info("certification>>> status.......................pass");
                                szStatus = "pass";
                                 iPass += 1;
                            }
                        }

                        // The command is expected to fail...
                        else if (jsonlookupTest.Get(szExpects + ".success") == "false")
                        {
                            // Check success...
                            if (string.IsNullOrEmpty(jsonlookupReply.Get(szPath + "results.success")))
                            {
                                Log.Info("certification>>> status.......................fail (missing " + szPath + "results.success)");
                                szStatus = "fail (missing " + szPath + "results.success)";
                                iFail += 1;
                            }
                            else if (jsonlookupReply.Get(szPath + "results.success") != "false")
                            {
                                Log.Info("certification>>> status.......................fail (expected " + szPath + "results.success to be 'false')");
                                szStatus = "fail (expected " + szPath + "results.success to be 'false')";
                                iFail += 1;
                            }

                            // Check the code...
                            else
                            {
                                switch (jsonlookupTest.Get(szExpects + ".code"))
                                {
                                    // Tell the programmer to fix their code or their tests...  :)
                                    default:
                                        Log.Info("certification>>> status.......................fail (no handler for this code '" + jsonlookupTest.Get(szExpects + ".code") + "')");
                                        iFail += 1;
                                        break;

                                    // JSON violations...
                                    case "invalidJson":
                                        if (string.IsNullOrEmpty(jsonlookupReply.Get(szPath + "results.code")))
                                        {
                                            Log.Info("certification>>> status.......................fail (missing " + szPath + "results.code)");
                                            szStatus = "fail (missing " + szPath + "results.code)";
                                            iFail += 1;
                                        }
                                        else if (jsonlookupReply.Get(szPath + "results.code") == "invalidJson")
                                        {
                                            if (string.IsNullOrEmpty(jsonlookupTest.Get(szExpects + ".characterOffset")))
                                            {
                                                Log.Info("certification>>> status.......................fail (missing " + szExpects + ".characterOffset)");
                                                szStatus = "fail (missing " + szExpects + ".characterOffset)";
                                                iFail += 1;
                                            }
                                            else if (int.Parse(jsonlookupTest.Get(szExpects + ".characterOffset")) == (int.Parse(jsonlookupReply.Get(szPath + "results.characterOffset")) - lTaskIndex))
                                            {
                                                Log.Info("certification>>> status.......................pass");
                                                szStatus = "pass";
                                                iPass += 1;
                                            }
                                            else
                                            {
                                                Log.Info("certification>>> status.......................fail (" + szExpects + ".characterOffset wanted:" + jsonlookupTest.Get(szExpects + ".characterOffset") + " got:" + (int.Parse(jsonlookupReply.Get(szPath + "results.characterOffset")) - lTaskIndex).ToString() + ")");
                                                szStatus = "fail (" + szExpects + ".characterOffset wanted:" + jsonlookupTest.Get(szExpects + ".characterOffset") + " got:" + (int.Parse(jsonlookupReply.Get(szPath + "results.characterOffset")) - lTaskIndex).ToString() + ")";
                                                iFail += 1;
                                            }
                                        }
                                        else
                                        {
                                            Log.Info("certification>>> status.......................fail (" + szExpects + ".code wanted:" + jsonlookupTest.Get(szExpects + ".code") + " got:" + jsonlookupReply.Get(szPath + "results.code") + ")");
                                            szStatus = "fail (" + szExpects + ".code wanted:" + jsonlookupTest.Get(szExpects + ".code") + " got:" + jsonlookupReply.Get(szPath + "results.code") + "')";
                                            iFail += 1;
                                        }
                                        break;

                                    // TWAIN Direct violations...
                                    case "invalidTask":
                                        if (string.IsNullOrEmpty(jsonlookupReply.Get(szPath + "results.code")))
                                        {
                                            Log.Info("certification>>> status.......................fail (missing " + szPath + "results.code)");
                                            szStatus = "fail (missing " + szPath + "results.code)";
                                            iFail += 1;
                                        }
                                        else if (jsonlookupReply.Get(szPath + "results.code") == "invalidTask")
                                        {
                                            if (string.IsNullOrEmpty(jsonlookupTest.Get(szExpects + ".jsonKey")))
                                            {
                                                Log.Info("certification>>> status.......................fail (missing " + szExpects + ".jsonKey)");
                                                szStatus = "fail (missing " + szExpects + "jsonKey)";
                                                iFail += 1;
                                            }
                                            else if (jsonlookupTest.Get(szExpects + ".jsonKey") == jsonlookupReply.Get(szPath + "results.jsonKey"))
                                            {
                                                Log.Info("certification>>> status.......................pass");
                                                szStatus = "pass";
                                                iPass += 1;
                                            }
                                            else
                                            {
                                                Log.Info("certification>>> status.......................fail (" + szExpects + ".jsonKey wanted:" + jsonlookupTest.Get(szExpects + ".jsonKey") + " got:" + jsonlookupReply.Get(szPath + "results.jsonKey"));
                                                szStatus = "fail (" + szExpects + ".jsonKey wanted:" + jsonlookupTest.Get(szExpects + ".jsonKey") + " got:" + jsonlookupReply.Get(szPath + "results.jsonKey");
                                                iFail += 1;
                                            }
                                        }
                                        else
                                        {
                                            Log.Info("certification>>> status.......................fail (" + szExpects + ".code wanted:" + jsonlookupTest.Get(szExpects + ".code") + " got:" + jsonlookupReply.Get(szPath + "results.code") + ")");
                                            szStatus = "fail (" + szExpects + ".code wanted:" + jsonlookupTest.Get(szExpects + ".code") + " got:" + jsonlookupReply.Get(szPath + "results.code") + "')";
                                            iFail += 1;
                                        }
                                        break;

                                    // invalidValue forced by exception...
                                    case "invalidValue":
                                        if (string.IsNullOrEmpty(jsonlookupReply.Get(szPath + "results.code")))
                                        {
                                            Log.Info("certification>>> status.......................fail (missing " + szPath + "results.code)");
                                            szStatus = "fail (missing " + szPath + "results.code)";
                                            iFail += 1;
                                        }
                                        else if (jsonlookupReply.Get(szPath + "results.code") == "invalidValue")
                                        {
                                            if (string.IsNullOrEmpty(jsonlookupTest.Get(szExpects + ".jsonKey")))
                                            {
                                                Log.Info("certification>>> status........................fail (missing " + szExpects + ".jsonKey)");
                                                szStatus = "fail (missing " + szExpects + ".jsonKey)";
                                                iFail += 1;
                                            }
                                            else if (jsonlookupTest.Get(szExpects + ".jsonKey") == jsonlookupReply.Get(szPath + "results.jsonKey"))
                                            {
                                                Log.Info("certification>>> status.......................pass");
                                                szStatus = "pass";
                                                iPass += 1;
                                            }
                                            else
                                            {
                                                Log.Info("certification>>> status.......................fail (" + szExpects + ".jsonKey wanted:" + jsonlookupTest.Get(szExpects + ".jsonKey") + " got:" + jsonlookupReply.Get(szPath + "results.jsonKey"));
                                                szStatus = "fail (" + szExpects + ".jsonKey wanted:" + jsonlookupTest.Get(szExpects + ".jsonKey") + " got:" + jsonlookupReply.Get(szPath + "results.jsonKey");
                                                iFail += 1;
                                            }
                                        }
                                        else
                                        {
                                            Log.Info("certification>>> status.......................fail (" + szExpects + ".code wanted:" + jsonlookupTest.Get(szExpects + ".code") + " got:" + jsonlookupReply.Get(szPath + "results.code") + ")");
                                            szStatus = "fail (" + szExpects + ".code wanted:" + jsonlookupTest.Get(szExpects + ".code") + " got:" + jsonlookupReply.Get(szPath + "results.code") + "')";
                                            iFail += 1;
                                        }
                                        break;
                                }
                            }
                        }

                        // Oops...
                        else
                        {
                            Log.Info("certification>>> status.......................fail (expectedSuccess must be 'true' or 'false')");
                            szStatus = "fail";
                            iFail += 1;
                        }
                    }
                }
            }

            // Pass count...
            Log.Info("certification>>> PASS: " + iPass);

            // Fail count...
            Log.Info("certification>>> FAIL: " + iFail);

            // Skip count...
            Log.Info("certification>>> SKIP: " + iSkip);

            // Total count...
            Log.Info("certification>>> TOTAL: " + iTotal);
        }

        #endregion


        // Private Definitions
        #region Private Definitions

        /// <summary>
        /// A key/value pair...
        /// </summary>
        private struct KeyValue
        {
            /// <summary>
            /// Our key...
            /// </summary>
            public string szKey;

            /// <summary>
            /// The key's value...
            /// </summary>
            public string szValue;
        }

        /// <summary>
        /// Call stack info...
        /// </summary>
        private struct CallStack
        {
            /// <summary>
            /// The arguments to this call...
            /// </summary>
            public Interpreter.FunctionArguments functionarguments;
        }

        #endregion


        // Private Attributes
        #region Private Attributes

        /// <summary>
        /// Map commands to functions...
        /// </summary>
        private List<Interpreter.DispatchTable> m_ldispatchtable;

        /// <summary>
        /// A snapshot of the current available devices...
        /// </summary>
        private Dnssd.DnssdDeviceInfo[] m_adnssddeviceinfoSnapshot;

        /// <summary>
        /// Information about our device...
        /// </summary>
        private Dnssd.DnssdDeviceInfo m_dnssddeviceinfoSelected;

        /// <summary>
        /// The connection to our device...
        /// </summary>
        private TwainLocalScanner m_twainlocalscanner;

        /// <summary>
        /// Our object for discovering TWAIN Local scanners...
        /// </summary>
        private Dnssd m_dnssd;

        /// <summary>
        /// No output when this is true...
        /// </summary>
        private bool m_blSilent;

        /// <summary>
        /// A record of the last transaction on the API, this
        /// doesn't include events...
        /// </summary>
        private ApiCmd.Transaction m_transactionLast;

        /// <summary>
        /// The list of key/value pairs created by the SET command...
        /// </summary>
        private List<KeyValue> m_lkeyvalue;

        /// <summary>
        /// A last in first off stack of function calls...
        /// </summary>
        private List<CallStack> m_lcallstack;

        #endregion
    }
}
