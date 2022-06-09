using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;

public class GameManager : MonoBehaviour
{
    SingleObject clickObj1 = null;
    SingleObject clickObj2 = null;

    [SerializeField]
    List<SingleObject> objs;

    Point[,] pointSourceArray = new Point[3, 3];

    // Start is called before the first frame update
    void Start()
    {
        
        for(var i = 0; i < 3; ++i)
        {
            for (var j = 0; j < 3; ++j)
            {
                var x = i;
                var y = j;

                var obj = objs.Find(o => o.MyPoint.X == x && o.MyPoint.Y == y);

                pointSourceArray[x, y] = obj.MyPoint;
            }
        }

        MessageBroker.Default.Receive<SingleObject.OnClickMessage>().Subscribe(
            obj =>
            {
                if (clickObj1 == null)
                    clickObj1 = obj.MyObject;
                else if (clickObj2 == null)
                {
                    clickObj2 = obj.MyObject;

                    Point[] pointArr = new Point[2] { 
                        clickObj1.MyPoint, 
                        clickObj2.MyPoint 
                    };

                    var testSingle = CardSearch.SingeSearch(pointSourceArray, clickObj1.MyPoint, clickObj2.MyPoint);
                    var testDouble = CardSearch.DoubleSearch(pointSourceArray, clickObj1.MyPoint, clickObj2.MyPoint);

                    Debug.Log(string.Format("testSingle:{0} testDouble:{1}", testSingle, testDouble));

                }
                else
                {

                    clickObj1 = null;
                    clickObj2 = null;

                    clickObj1 = obj.MyObject;
                }
            });
    }
}


[System.Serializable]
public class Point
{
    int x, y;
    bool flag;
    public Point(int _x, int _y)
    {
        x = _x;
        y = _y;
        flag = false;
    }

    public int X
    {
        get
        {
            return x;
        }
    }

    public int Y
    {
        get
        {
            return y;
        }
    }

    /// <summary>
    /// 判断这个点可不可以走
    /// </summary>
    /// <returns></returns>
    public bool IsMove()
    {
        return flag;
    }



    public void SetStatus(bool bol)
    {
        flag = bol;
    }
    public override string ToString()
    {
        return string.Format("x: {0},y:{1} flag:{2}", x, y, flag);
    }
}



public class CardSearch
{
    //0拐角查找
    /// <summary>
    /// 单线查找
    /// </summary>
    /// <param name="sourceArray">查找的数组</param>
    /// <param name="startPoint"> 开始点 </param>
    /// <param name="endPoint"> 结束点 </param>
    /// <returns></returns>
    public static bool SingeSearch(Point[,] sourceArray, Point startPoint, Point endPoint)
    {
        if (startPoint.X != endPoint.X && startPoint.Y != endPoint.Y)
        {
            return false;
        }

        int i;
        //水平
        if (startPoint.X != endPoint.X)
        {
            int min = startPoint.X < endPoint.X ? startPoint.X : endPoint.X;

            int max = startPoint.X > endPoint.X ? startPoint.X : endPoint.X;


            for (i = min + 1; i < max; i++)
            {
                if (!sourceArray[i, startPoint.Y].IsMove())
                {
                    return false;
                }
            }
            return true;
        }

        //竖直

        if (startPoint.Y != endPoint.Y)
        {
            //求Y轴的最小值
            int min = startPoint.Y > endPoint.Y ? endPoint.Y : startPoint.Y;
            //求Y轴的最大值
            int max = startPoint.Y < endPoint.Y ? endPoint.Y : startPoint.Y;

            for (i = min + 1; i < max; i++)
            {
                if (!sourceArray[startPoint.X, i].IsMove())
                {
                    return false;
                }
            }
            return true;
        }

        return false;


    }


    //1拐角查找
    /// <summary>
    /// 双线查找
    /// </summary>
    /// <param name="sourceArray">查找的数组</param>
    /// <param name="startPoint"> 开始点 </param>
    /// <param name="endPoint"> 结束点 </param>
    /// <returns></returns>
    public static Point DoubleSearch(Point[,] sourceArray, Point startPoint, Point endPoint)
    {

        if (startPoint.X == endPoint.X || startPoint.Y == endPoint.Y)
        {
            return null;
        }

        Point pt = sourceArray[startPoint.X, endPoint.Y];
        //开始点在左面的时候
        //这个点能走
        if (pt.IsMove())
        {
            //如果开始点能走到拐角点
            if (SingeSearch(sourceArray, startPoint, pt))
            {
                if (SingeSearch(sourceArray, pt, endPoint))
                {
                    return pt;
                }
            }
        }

        //测试拐角点二
        pt = sourceArray[endPoint.X, startPoint.Y];

        if (pt.IsMove())
        {
            //如果开始点能走到拐角点
            if (SingeSearch(sourceArray, startPoint, pt))
            {
                //如果拐角点能走到结束点
                if (SingeSearch(sourceArray, pt, endPoint))
                {
                    return pt;
                }
            }
        }
        return null;

    }


    public static List<Point> MulSearch(Point[,] sourceArray, Point startPoint, Point endPoint)
    {
        if (sourceArray == null || sourceArray.Length == 0)
        {
            Debug.LogError("原数组为空");
            return null;
        }

        if (startPoint.X == endPoint.X && endPoint.Y == startPoint.Y)
        {
            //开始点和结束点相同
            Debug.Log("开始点和结束点相同");
            return null;
        }


        //判断开始点和结束点是否符合越界
        if (startPoint.X < 0 || startPoint.Y >= sourceArray.Length || endPoint.X < 0 || startPoint.Y >= sourceArray.Length)
        {

            Debug.Log("数组越界" + sourceArray.GetLength(0) + "    ");
            return null;
        }
        int i;
        List<Point> path = new List<Point>();

        path.Add(startPoint);

        //在结束点的x轴（水平方向）查找一个点 //Y轴相同
        for (i = 0; i < sourceArray.GetLength(0); i++)
        {
            //获取到水平方向上的点
            Point point = sourceArray[i, endPoint.Y];
            Debug.Log("取到的临时拐角点:" + point);
            //如果这个点不是结束点
            if (point.X != endPoint.X)
            {
                //如果这个点可以走
                if (point.IsMove())
                {
                    //然后以这个点为开始点，单线查找 到结束点
                    if (SingeSearch(sourceArray, point, endPoint))
                    {
                        //如果单线查找成功  判断双线查找
                        Point singlePoint = DoubleSearch(sourceArray, startPoint, point);
                        if (singlePoint != null)
                        {
                            //如果双线查找成功，查找路径成功 将路径列表返回
                            path.Add(singlePoint);
                            path.Add(point);
                            path.Add(endPoint);

                            return path;
                        }

                    }
                }
            }
        }


        //判断竖直轴
        //在结束点的Y轴（竖直方向）查找一个点 //X轴相同
        for (i = 0; i < sourceArray.GetLength(1); i++)
        {
            //在竖直方向上取一个点
            Point point = sourceArray[endPoint.X, i];
            //如果点不是结束点
            if (point.Y != endPoint.Y)
            {
                //如果这个点能走
                if (point.IsMove())
                {
                    //如果单线查找成功
                    if (SingeSearch(sourceArray, point, endPoint))
                    {
                        //从开始点查找到这个点 
                        Point singlePoint = DoubleSearch(sourceArray, startPoint, point);
                        //查找成功
                        if (singlePoint != null)
                        {
                            path.Add(singlePoint);
                            path.Add(point);
                            path.Add(endPoint);
                            return path;
                        }
                    }
                }
            }
        }



        return null;
    }
}