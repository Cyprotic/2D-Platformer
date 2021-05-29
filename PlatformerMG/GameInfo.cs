using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PlatformerMG
{
    public class EnemyInfo
    {
        public float Speed = 0.0f;
        public float Speed2 = 0.0f;
        public float Speed3 = 0.0f;
    }

    public class GemInfo
    {
        public float BounceHeight = 0.0f;
        public float BounceRate = 0.0f;
        public Color Color;
    }

    public class GameInfo
    {
        private static GameInfo mInstance = null;
        public static GameInfo Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new GameInfo();
                return mInstance;
            }

            set { mInstance = value; }
        }

        public EnemyInfo EnemyInfo;
        public GemInfo GemInfo;
    }
}
