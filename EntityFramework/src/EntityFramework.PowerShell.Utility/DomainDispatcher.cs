﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Management.Automation;
    using EnvDTE;

    /// <summary>
    ///     Provides a way of dispatching specific calls form the PowerShell commands'
    ///     AppDomain to the Visual Studio's main AppDomain.
    /// </summary>
    [CLSCompliant(false)]
    public class DomainDispatcher : MarshalByRefObject
    {
        private readonly PSCmdlet _cmdlet;
        private readonly DTE _dte;

        public DomainDispatcher()
        {
            // Testing    
        }

        public DomainDispatcher(PSCmdlet cmdlet)
        {
            // Not using Check here because this assembly is very small and without resources
            if (cmdlet == null)
            {
                throw new ArgumentNullException("cmdlet");
            }

            _cmdlet = cmdlet;
            _dte = (DTE)cmdlet.GetVariableValue("DTE");
        }

        public void WriteLine(string text)
        {
            // Not using Check here because this assembly is very small and without resources
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text");
            }

            _cmdlet.Host.UI.WriteLine(text);
        }

        public void WriteWarning(string text)
        {
            // Not using Check here because this assembly is very small and without resources
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text");
            }

            _cmdlet.WriteWarning(text);
        }

        public void WriteVerbose(string text)
        {
            // Not using Check here because this assembly is very small and without resources
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException("text");
            }

            _cmdlet.WriteVerbose(text);
        }

        public virtual void OpenFile(string fileName)
        {
            // Not using Check here because this assembly is very small and without resources
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            _dte.ItemOperations.OpenFile(fileName);
        }

        public void NewTextFile(string text, string item = @"General\Text File")
        {
            var window = _dte.ItemOperations.NewFile(item);
            var textDocument = (TextDocument)window.Document.Object("TextDocument");
            var editPoint = textDocument.StartPoint.CreateEditPoint();
            editPoint.Insert(text);
        }
    }
}
