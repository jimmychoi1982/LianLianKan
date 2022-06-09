using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class SingleObject : MonoBehaviour
{
    public class OnClickMessage
    {
        public SingleObject MyObject;
    }

    [SerializeField]
    Point point;
    public Point MyPoint => point;

    [SerializeField]
    Vector2 position = new Vector2();
    public Vector2 Position => position;

    // Start is called before the first frame update
    void Start()
    {
        //point = new Point((int)position.x, (int)position.y);
    }

    private void Awake()
    {
        point = new Point((int)position.x, (int)position.y);
    }

    public void OnClick()
    {
        MessageBroker.Default.Publish(new OnClickMessage { MyObject = this });
    }
}
