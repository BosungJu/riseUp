using System;
using System.Collections;
using UnityEngine;

namespace BSLibrary
{
    [System.Serializable]
    public class Tween : MonoBehaviour
    {
        public enum TweenType
        {
            Linear = 0,
            InSine = 1,
            OutSine = 2,
            InOutSine = 3,
            InQuad = 4,
            OutQuad = 5,
            InOutQuad = 6,
            InCubic = 7,
            OutCubic = 8,
            InOutCubic = 9,
            InQuart = 10,
            OutQuart = 11,
            InOutQuart = 12,
            InQuint = 13,
            OutQuint = 14,
            InOutQuint = 15,
            InExpo = 16,
            OutExpo = 17,
            InOutExpo = 18,
            InCirc = 19,
            OutCirc = 20,
            InOutCirc = 21,
            InBack = 22,
            OutBack = 23,
            InOutBack = 24,
            InElastic = 25,
            OutElastic = 26,
            InOutElastic = 27,
            InBounce = 28,
            OutBounce = 29,
            InOutBounce = 30
        }

        public enum LoopType
        {
            Restart = 0, Yoyo = 1
        }
        
        public TweenType Ease;
        public LoopType _LoopType;

        public bool IsRepeat;
        public bool IsPause;
        public float RepeatTime;

        private bool _isIncrease = true;
        public float ReturnValueToFloat
        {
            get { return (float)ReturnValue; }
        }

        public bool IsPlay { get; private set; }
        public double ReturnValue { get; private set; }
        public float _Time { get; private set; }
        
        private Coroutine _coroutine;
        private bool _isMotionComplete = false;
        
        #region TweenFunctions

        public static double Linear(double x)
        {
            return x;
        }

        public static double InSine(double x)
        {
            return 1 - Math.Cos(x * Mathf.PI / 2);
        }

        public static double OutSine(double x)
        {
            return 1 - Math.Sin(x * Mathf.PI / 2);
        }

        public static double InOutSine(double x)
        {
            return -(Math.Cos(Mathf.PI * x) - 1) / 2;
        }

        public static double InQuad(double x)
        {
            return Math.Pow(x, 2);
        }

        public static double OutQuad(double x)
        {
            return 1 - Math.Pow(1 - x, 2);
        }

        public static double InOutQuad(double x)
        {
            return x < 0.5 ? 2 * Math.Pow(x, 2) : 1 - Math.Pow(-2 * x + 2, 2) / 2;
        }

        public static double InCubic(double x)
        {
            return Math.Pow(x, 3);
        }

        public static double OutCubic(double x)
        {
            return 1 - Math.Pow(1 - x, 3);
        }

        public static double InOutCubic(double x)
        {
            return x < 0.5 ? 4 * Math.Pow(x, 3) : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }

        public static double InQuart(double x)
        {
            return Math.Pow(x, 4);
        }

        public static double OutQuart(double x)
        {
            return 1 - Math.Pow(1 - x, 4);
        }

        public static double InOutQuart(double x)
        {
            return x < 0.5 ? 8 * Math.Pow(x, 4) : 1 - Math.Pow(-2 * x + 2, 4) / 2;
        }

        public static double InQuint(double x)
        {
            return Math.Pow(x, 5);
        }

        public static double OutQuint(double x)
        {
            return 1 - Math.Pow(1 - x, 5);
        }

        public static double InOutQuint(double x)
        {
            return x < 0.5 ? 16 * Math.Pow(x, 5) : 1 - Math.Pow(-2 * x + 2, 5) / 2;
        }

        public static double InExpo(double x)
        {
            return x == 0 ? 0 : Math.Pow(2, 10 * x - 10);
        }

        public static double OutExpo(double x)
        {
            return x == 1 ? 1 : 1 - Math.Pow(2, -10 * x);
        }

        public static double InOutExpo(double x)
        {
            return x == 0
                ? 0
                : x == 1
                    ? 1
                    : x < 0.5
                        ? Math.Pow(2, 20 * x - 10) / 2
                        : (2 - Math.Pow(2, -20 * x + 10)) / 2;
        }

        public static double InCirc(double x)
        {
            return 1 - Math.Sqrt(1 - Math.Pow(x, 2));
        }

        public static double OutCirc(double x)
        {
            return Math.Sqrt(1 - Math.Pow(x - 1, 2));
        }

        public static double InOutCirc(double x)
        {
            return x < 0.5
                ? (1 - Math.Sqrt(1 - Math.Pow(2 * x, 2))) / 2
                : (Math.Sqrt(1 - Math.Pow(-2 * x + 2, 2)) + 1) / 2;
        }

        public static double InBack(double x)
        {
            const double c1 = 1.70158;
            const double c3 = c1 + 1;

            return c3 * Math.Pow(x, 3) - c1 * Math.Pow(x, 2);
        }

        public static double OutBack(double x)
        {
            const double c1 = 1.70158;
            const double c3 = c1 + 1;

            return 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2);
        }

        public static double InOutBack(double x)
        {
            const double c1 = 1.70158;
            const double c2 = c1 * 1.525;

            return x < 0.5
                ? (Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
                : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
        }

        public static double InElastic(double x)
        {
            const double c4 = (2 * Math.PI) / 3;

            return x == 0
                ? 0
                : x == 1
                    ? 1
                    : -Math.Pow(2, 10 * x - 10) * Math.Sin((x * 10 - 10.75) * c4);
        }

        public static double OutElastic(double x)
        {
            const double c4 = (2 * Math.PI) / 3;

            return x == 0
                ? 0
                : x == 1
                    ? 1
                    : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1;
        }

        public static double InOutElastic(double x)
        {
            const double c5 = (2 * Math.PI) / 4.5;

            return x == 0
                ? 0
                : x == 1
                    ? 1
                    : x < 0.5
                        ? -(Math.Pow(2, 20 * x - 10) * Math.Sin((20 * x - 11.125) * c5)) / 2
                        : (Math.Pow(2, -20 * x + 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 + 1;
        }

        public static double InBounce(double x)
        {
            return 1 - OutBounce(1 - x);
        }

        public static double OutBounce(double x)
        {
            const double n1 = 7.5625;
            const double d1 = 2.75;

            if (x < 1 / d1)
            {
                return n1 * x * x;
            }
            else if (x < 2 / d1)
            {
                return n1 * (x -= 1.5 / d1) * x + 0.75;
            }
            else if (x < 2.5 / d1)
            {
                return n1 * (x -= 2.25 / d1) * x + 0.9375;
            }
            else
            {
                return n1 * (x -= 2.625 / d1) * x + 0.984375;
            }
        }

        public static double InOutBounce(double x)
        {
            return x < 0.5
                ? (1 - OutBounce(1 - 2 * x)) / 2
                : (1 + OutBounce(2 * x - 1)) / 2;
        }

        #endregion

        private double Play(double x)
        {
            switch (Ease)
            {
                case TweenType.Linear: return Linear(x);
                case TweenType.InSine: return InSine(x);
                case TweenType.OutSine: return OutSine(x);
                case TweenType.InOutSine: return InOutSine(x);
                case TweenType.InQuad: return InQuad(x);
                case TweenType.OutQuad: return OutQuad(x);
                case TweenType.InOutQuad: return InOutQuad(x);
                case TweenType.InCubic: return InCubic(x);
                case TweenType.OutCubic: return OutCubic(x);
                case TweenType.InOutCubic: return InOutCubic(x);
                case TweenType.InQuart: return InQuart(x);
                case TweenType.OutQuart: return OutQuart(x);
                case TweenType.InOutQuart: return InOutQuart(x);
                case TweenType.InQuint: return InQuint(x);
                case TweenType.OutQuint: return OutQuint(x);
                case TweenType.InOutQuint: return InOutQuint(x);
                case TweenType.InExpo: return InExpo(x);
                case TweenType.OutExpo: return OutExpo(x);
                case TweenType.InOutExpo: return InOutExpo(x);
                case TweenType.InCirc: return InCirc(x);
                case TweenType.OutCirc: return OutCirc(x);
                case TweenType.InOutCirc: return InOutCirc(x);
                case TweenType.InBack: return InBack(x);
                case TweenType.OutBack: return OutBack(x);
                case TweenType.InOutBack: return InOutBack(x);
                case TweenType.InElastic: return InElastic(x);
                case TweenType.OutElastic: return OutElastic(x);
                case TweenType.InOutElastic: return InOutElastic(x);
                case TweenType.InBounce: return InBounce(x);
                case TweenType.OutBounce: return OutBounce(x);
                case TweenType.InOutBounce: return InOutBounce(x);
                default: return Double.NaN;
            }
        }

        private IEnumerator StartPlay()
        {
            while (true)
            {
                while (IsPause) { yield return new WaitForEndOfFrame(); }

                _Time += _isIncrease ? Time.deltaTime : -Time.deltaTime;
                _Time = _Time > RepeatTime ? RepeatTime : _Time;
                ReturnValue = Play(_Time / RepeatTime);
                yield return new WaitForEndOfFrame();

                if (_Time >= RepeatTime && _isIncrease || _Time <= 0 && !_isIncrease)
                {
                    if (!IsRepeat && _LoopType != LoopType.Yoyo || _isMotionComplete && !IsRepeat)
                    {
                        StopCoroutine();
                    }
                    else if (!IsRepeat)
                    {
                        _isMotionComplete = true;
                    }
                    else
                    {
                        switch (_LoopType)
                        {
                            case LoopType.Restart:
                                ReturnValue = 0;
                                _Time = 0;
                                _isIncrease = true;
                                break;
                            case LoopType.Yoyo:
                                if (_Time >= RepeatTime)
                                {
                                    ReturnValue = 1;
                                    _Time = RepeatTime;
                                    _isIncrease = false;
                                }
                                else if (_Time <= 0)
                                {
                                    ReturnValue = 0;
                                    _Time = 0;
                                    _isIncrease = true;
                                }
                                break;
                        }
                    }
                }
            }
        }

        public void StartCoroutine()
        {
            IsPause = false;
            _coroutine = StartCoroutine(StartPlay());
            IsPlay = true;
        }

        public void StopCoroutine()
        {
            StopCoroutine(_coroutine);
            _isIncrease = true;
            _Time = 0;
            IsPlay = false;
        }

        public void SetTween(TweenType tweenType, LoopType loopType, bool isRepeat, float repeatTime)
        {
            Ease = tweenType;
            _LoopType = loopType;
            IsRepeat = isRepeat;
            RepeatTime = repeatTime;
            _isIncrease = true;
            _Time = 0;
            _isMotionComplete = false;
            ReturnValue = 0;
            IsPlay = false;
            IsPause = false;
        }
    }
}
