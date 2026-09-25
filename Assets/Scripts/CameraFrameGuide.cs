using UnityEngine;

[ExecuteAlways]
public class MultiCameraFrameGuide : MonoBehaviour
{
    public enum VerticalAnchor { Top, Center, Bottom }

    [Header("Format cible")]
    [SerializeField] private Vector2 targetResolution = new Vector2(1920f, 513f);
    [SerializeField] private VerticalAnchor verticalAnchor = VerticalAnchor.Top;

    [Header("Caméras cibles")]
    [Tooltip("Laisser vide pour détecter automatiquement toutes les caméras de la scène")]
    [SerializeField] private Camera[] customCameras;

    [Header("Profondeur")]
    [SerializeField] private float nearDistance = 1.0f;
    [SerializeField] private float farDistance = 5.0f;
    [SerializeField] private bool drawAsVolume = false;

    [Header("Affichage")]
    [SerializeField] private bool showCameraFrustum = true;
    [SerializeField] private Color cameraFrameColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color guideColor = Color.cyan;

    private void OnDrawGizmos()
    {
        Camera[] cams = (customCameras != null && customCameras.Length > 0)
            ? customCameras
            : Camera.allCameras;

        if (cams == null) return;

        foreach (Camera cam in cams)
        {
            if (cam == null || !cam.isActiveAndEnabled) continue;

            if (showCameraFrustum)
            {
                Gizmos.color = cameraFrameColor;
                DrawViewportRect(cam, 0f, 1f, 0f, 1f);
            }

            Gizmos.color = guideColor;
            DrawTargetFrame(cam);
        }
    }

    private void DrawTargetFrame(Camera cam)
    {
        float targetAspect = targetResolution.x / targetResolution.y;
        float camAspect = cam.aspect;

        float w = 1f;
        float h = 1f;

        if (targetAspect > camAspect)
            h = camAspect / targetAspect;
        else
            w = targetAspect / camAspect;

        float minX = 0.5f - w * 0.5f;
        float maxX = 0.5f + w * 0.5f;

        float minY, maxY;
        switch (verticalAnchor)
        {
            case VerticalAnchor.Top:
                maxY = 1f;
                minY = 1f - h;
                break;
            case VerticalAnchor.Bottom:
                minY = 0f;
                maxY = h;
                break;
            case VerticalAnchor.Center:
            default:
                minY = 0.5f - h * 0.5f;
                maxY = 0.5f + h * 0.5f;
                break;
        }

        DrawViewportRect(cam, minX, maxX, minY, maxY);
    }

    private void DrawViewportRect(Camera cam, float minX, float maxX, float minY, float maxY)
    {
        Vector3 p0Near = cam.ViewportToWorldPoint(new Vector3(minX, minY, nearDistance));
        Vector3 p1Near = cam.ViewportToWorldPoint(new Vector3(maxX, minY, nearDistance));
        Vector3 p2Near = cam.ViewportToWorldPoint(new Vector3(maxX, maxY, nearDistance));
        Vector3 p3Near = cam.ViewportToWorldPoint(new Vector3(minX, maxY, nearDistance));

        Gizmos.DrawLine(p0Near, p1Near);
        Gizmos.DrawLine(p1Near, p2Near);
        Gizmos.DrawLine(p2Near, p3Near);
        Gizmos.DrawLine(p3Near, p0Near);

        if (drawAsVolume)
        {
            Vector3 p0Far = cam.ViewportToWorldPoint(new Vector3(minX, minY, farDistance));
            Vector3 p1Far = cam.ViewportToWorldPoint(new Vector3(maxX, minY, farDistance));
            Vector3 p2Far = cam.ViewportToWorldPoint(new Vector3(maxX, maxY, farDistance));
            Vector3 p3Far = cam.ViewportToWorldPoint(new Vector3(minX, maxY, farDistance));

            Gizmos.DrawLine(p0Far, p1Far);
            Gizmos.DrawLine(p1Far, p2Far);
            Gizmos.DrawLine(p2Far, p3Far);
            Gizmos.DrawLine(p3Far, p0Far);

            Gizmos.DrawLine(p0Near, p0Far);
            Gizmos.DrawLine(p1Near, p1Far);
            Gizmos.DrawLine(p2Near, p2Far);
            Gizmos.DrawLine(p3Near, p3Far);
        }
    }
}