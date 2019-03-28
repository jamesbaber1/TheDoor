using UnityEngine;
using System.Collections;

public class KinectOverlayer : MonoBehaviour 
{
    public float y_rotation_intensity = 3.0f;
    public float x_rotation_intensity = 1.0f;


    public GUITexture backgroundImage;
	public KinectWrapper.NuiSkeletonPositionIndex TrackedJoint = KinectWrapper.NuiSkeletonPositionIndex.HandRight;
	public GameObject OverlayObject;
    public float smoothFactor = 25f;//5f;
	
	public GUIText debugText;

    public Camera cam;

    //public Transform camPos;

	private float distanceToCamera = 10f;


	void Start()
	{
        //camPos = Camera.main.transform;

        if (OverlayObject)
		{
			distanceToCamera = (OverlayObject.transform.position - Camera.main.transform.position).magnitude;
		}
	}
	
	void Update() 
	{
		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{
			//backgroundImage.renderer.material.mainTexture = manager.GetUsersClrTex();
			if(backgroundImage && (backgroundImage.texture == null))
			{
				backgroundImage.texture = manager.GetUsersClrTex();
			}
			
//			Vector3 vRight = BottomRight - BottomLeft;
//			Vector3 vUp = TopLeft - BottomLeft;
			
			int iJointIndex = (int)TrackedJoint;
			
			if(manager.IsUserDetected())
			{
				uint userId = manager.GetPlayer1ID();
				
				if(manager.IsJointTracked(userId, iJointIndex))
				{
					Vector3 posJoint = manager.GetRawSkeletonJointPos(userId, iJointIndex);

					if(posJoint != Vector3.zero)
					{
						// 3d position to depth
						Vector2 posDepth = manager.GetDepthMapPosForJointPos(posJoint);
						
						// depth pos to color pos
						Vector2 posColor = manager.GetColorMapPosForDepthPos(posDepth);
						
						float scaleX = (float)posColor.x / KinectWrapper.Constants.ColorImageWidth;
						float scaleY = 1.0f - (float)posColor.y / KinectWrapper.Constants.ColorImageHeight;
						
//						Vector3 localPos = new Vector3(scaleX * 10f - 5f, 0f, scaleY * 10f - 5f); // 5f is 1/2 of 10f - size of the plane
//						Vector3 vPosOverlay = backgroundImage.transform.TransformPoint(localPos);
						//Vector3 vPosOverlay = BottomLeft + ((vRight * scaleX) + (vUp * scaleY));

						if(debugText)
						{
							debugText.GetComponent<GUIText>().text = "Tracked user ID: " + userId;  // new Vector2(scaleX, scaleY).ToString();
						}
						
						if(OverlayObject)
						{
                            Vector3 vPosOverlay = Camera.main.ViewportToWorldPoint(new Vector3(scaleX, scaleY, distanceToCamera));
                            OverlayObject.transform.position = Vector3.Lerp(OverlayObject.transform.position, vPosOverlay, smoothFactor * Time.deltaTime);
                            //COPYLOCATION ? <<- lookup
                            //Vector3 ballPos = new Vector3();
                            //ballPos.Set(OverlayObject.transform.position.x, OverlayObject.transform.position.y, OverlayObject.transform.position.z);
                            float y = OverlayObject.transform.position.y;
                            float x = OverlayObject.transform.position.x;
                            if (OverlayObject.transform.position.y < 1.0f)
                            {
                                y = 1.0f;
                            }
                            
                            if(x < -5.0f)
                            {
                                x = -5.0f;
                            }
                            else if(x > 5.0f)
                            {
                                x = 5.0f;
                            }
                            
                            OverlayObject.transform.position = new Vector3(x, y, OverlayObject.transform.position.z);
                            
                            OverlayObject.transform.rotation = Quaternion.Euler(new Vector3(OverlayObject.transform.position.y * y_rotation_intensity, -OverlayObject.transform.position.x * x_rotation_intensity, 0));
                            //OverlayObject.transform.rotation = Quaternion.Euler(new Vector3(ballPos.y * 3.0f, ballPos.x * -3.0f, 0));

                            /*
                            //camPos.position = Vector3.Lerp(OverlayObject.transform.position, vPosOverlay, smoothFactor * Time.deltaTime);
                            left = -OverlayObject.transform.position.x;
                            right = OverlayObject.transform.position.x;
                            bottom = -OverlayObject.transform.position.y;
                            top = OverlayObject.transform.position.y;
                            Matrix4x4 m = PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
                            cam.projectionMatrix = m;
                            */
                        }
                    }
				}
				
			}
			
		}
	}
    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }

}
