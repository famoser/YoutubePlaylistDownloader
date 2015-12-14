using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using YoutubePlaylistDownloader.Business.Models;

namespace YoutubePlaylistDownloader.View.ViewModels
{
    public class ProgressViewModel : ViewModelBase
    {
        public ProgressViewModel()
        {
            if (IsInDesignMode)
            {
                _percentageProgressModels = new Dictionary<ProgressModel, int>();
                _percentageProgressModels.Add(new ProgressModel()
                {
                    Description = "Current progress..."
                }, 32);
            }
        }

        private Dictionary<ProgressModel, int> _percentageProgressModels = new Dictionary<ProgressModel, int>();

        public void SetProgress(ProgressModel pm, int progress)
        {
            if (!_percentageProgressModels.ContainsKey(pm))
            {
                _percentageProgressModels.Add(pm, 0);
            }

            _percentageProgressModels[pm] = progress;
            ActiveModel = pm;
        }

        private ProgressModel _activeModel;
        private ProgressModel ActiveModel
        {
            get { return _activeModel; }
            set
            {
                if (Set(ref _activeModel, value))
                {
                    RaisePropertyChanged(() => ActiveProgress);
                    RaisePropertyChanged(() => ProgressMessage);
                }
            }
        }
        public double ActiveProgress
        {
            get
            {
                if (ActiveModel != null)
                    return _percentageProgressModels[ActiveModel];
                return 0;
            }
        }

        public string ProgressMessage
        {
            get
            {
                return ActiveModel?.Description;
            }
        }

        public void RemoveProgress(ProgressModel pm)
        {
            if (_percentageProgressModels.ContainsKey(pm))
            {
                _percentageProgressModels.Remove(pm);
                ActiveModel = _percentageProgressModels.Count > 0 ? _percentageProgressModels.First().Key : null;
            }
        }
    }
}
