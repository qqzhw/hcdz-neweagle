using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvirtech.QyRound.Models
{
   public  class NetCardInfo: BindableBase
    {
        private string _dev_name;
        public string dev_name
        {
            get { return _dev_name; }
            set { SetProperty(ref _dev_name, value); }
        }

        private string _dev_description;
        public string dev_description
        {
            get { return _dev_description; }
            set { SetProperty(ref _dev_description, value); }
        }
    }
}
