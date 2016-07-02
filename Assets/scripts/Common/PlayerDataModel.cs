#if UNITY
using UnityEngine;
using ExitGames.Client.Photon;
#endif

using System.Collections;

namespace TapCommon {
	public class PlayerDataModel {
		private string name_;
        public string Name
        {
            get { return name_; }
            set { name_ = value; }
        }

        private double exp_;
        public double Exp
        {
            get { return exp_; }
            set { exp_ = value; }
        }

        private long id_;
        public long ID
        {
            get { return id_; }
            set { id_ = value; }
        }

		public PlayerDataModel() 
        {
		}
	}
}