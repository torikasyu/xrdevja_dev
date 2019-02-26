using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPointer : MonoBehaviour
{

    [SerializeField]
    private Transform _rightPointer = null;
    [SerializeField]
    private Transform _leftPointer = null;

    public float _maxDistance = 100f;

    // オブジェクトを掴む
    private GameObject _grabObject = null;
    private Vector3 _grabOffset;
    private float _grabDistance = 0;
    private Vector3 _gravPrevFramePos;

    // 現在アクティブな左右のどちらかのコントロールを得る
    private Transform GetController()
    {
        var controller = OVRInput.GetActiveController();
        //return (controller == OVRInput.Controller.RTrackedRemote ? _rightPointer : _leftPointer);

        if (controller == OVRInput.Controller.RTrackedRemote
            || controller == OVRInput.Controller.RTouch)
        {
            return _rightPointer;
        }
        else
        {
            return _leftPointer;
        }
    }

    public LayerMask grabLayer;

    // Update is called once per frame
    void Update()
    {
        var controller = GetController();

        // コントローラーから前に伸ばしたRayを作成
        var pointerRay = new Ray(controller.position, controller.transform.forward);

        // Rayがヒットした位置を取得
        RaycastHit hitInfo;
        if (Physics.Raycast(pointerRay, out hitInfo, _maxDistance,grabLayer))
        {
            // ヒットしたオブジェクトを取得
            GameObject hitObj = hitInfo.collider.gameObject;

            // トリガーボタンを押した時
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                if (hitObj.name != "Plane")
                { // 地面は掴めない
                    _grabObject = hitObj;
                    _grabDistance = hitInfo.distance;
                    _grabOffset = hitObj.transform.position - hitInfo.point; // ヒットした場所からオブジェクト中心までの距離
                    _gravPrevFramePos = hitObj.transform.position;
                }
            }

        }

        // 掴んだオブジェクトを移動
        if (_grabObject != null)
        {
            // 上下タッチで距離変更
            Vector2 pt = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad); ///タッチパッドの位置
            if (pt.y > +0.6 && (-0.5 < pt.x && pt.x < +0.5))  //上側？
            {
                _grabDistance += 0.1f;
            }
            else if (pt.y < -0.9 && (-0.5 < pt.x && pt.x < +0.5)) // 下側？
            {
                _grabDistance -= 0.1f;
                if (_grabDistance < 0.1) { _grabDistance = 0.1f; }
            }

            // 移動
            _gravPrevFramePos = _grabObject.transform.position;
            _grabObject.transform.position = pointerRay.GetPoint(_grabDistance) + _grabOffset;
            _grabObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        // 掴んだオブジェクトを離す
        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
        {
            // トリガーボタンを離した時
            Vector3 force = _grabObject.transform.position - _gravPrevFramePos;
            _grabObject.GetComponent<Rigidbody>().velocity = force * 30f;
            _grabObject = null;
        }

    }
}