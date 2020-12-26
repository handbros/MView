﻿using MView.Bases;
using MView.Core;
using MView.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MView.ViewModels.File
{
    public class GeneralFileViewModel : FileViewModelBase
    {
        #region ::Fields::

        private string _textContent = string.Empty;

        private FileProperties _fileProperties = new FileProperties();

        #endregion

        #region ::Constructors::

        public GeneralFileViewModel(string filePath) : base(filePath)
        {
            Initialize(filePath);
        }

        #endregion

        #region ::Properties::

        public string TextContent
        {
            get
            {
                return _textContent;
            }
            set
            {
                if (_textContent != value)
                {
                    _textContent = value;
                    RaisePropertyChanged(nameof(TextContent));

                    IsDirty = true;
                }
            }
        }

        public string FileSizeString
        {
            get
            {
                return _fileProperties.Size;
            }
        }

        #endregion

        #region ::Methods::

        private async void Initialize(string filePath)
        {
            var task = Task.Run(() =>
            {
                _textContent = FileManager.ReadTextFile(filePath, Encoding.UTF8);

                _fileProperties = new FileProperties(filePath);
            });

            await task;
        }

        #endregion
    }
}
