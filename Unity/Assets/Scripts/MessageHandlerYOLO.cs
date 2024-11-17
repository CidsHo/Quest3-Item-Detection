/*
 * ����ű����ڴ���ӻ���ѧϰģ�ͣ����� YOLOV8��ͨ���������ӽ��յ��ļ������
 * ����������������ݵ�JSON������Ϣ���������ݣ�����Ӧ�ظ���UI��
 * �ýű���������Ҫ��Unity��Ŀ�н���Ŀ������ڻ����Ͻ��п��ӻ��ĳ�����
 *
 * ���ܣ�
 * - ��������NetworkingClient�ļ����Ϣ��
 * - ��JSON��Ϣ����Ϊ����б�
 * - ��̬����UI����ʾ��⵽��Ŀ��ı߽���������ơ�
 * - ʹ��TextMeshPro�ڼ�������ʾ������ơ�
 * - �Զ�����ɵ�UIԪ�أ�ȷ��ֻ��ʾ���µļ������
 * - ���������ӶϿ�ʱ����߽��
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // ����TextMeshPro�������ռ䣬���ڴ���UI������ʾ

[Serializable]  // ���Ϊ�����л��������JSON�����н���
public class Detection
{
    public string class_name;  // ��⵽��Ŀ����������
    public List<int> bbox;  // �߽�����꣬����[x, y, x2, y2]��(x, y) �Ǿ��ο�����Ͻǵ㣬�������˾��ε���ʼλ�á�(x2, y2) �Ǿ��ο�����½ǵ㣬�������˾��ε���ֹλ�á�
}

[Serializable]  // ���Ϊ�����л��������JSON�����н���
public class DetectionWrapper
{
    public List<Detection> detections;  // ������������б�
}

public class MessageHandlerYOLO : MonoBehaviour
{
    public NetworkingClient networkingClient;  // ���ڽ�����Ϣ������ͻ���
    public GameObject imagePrefab;  // ������ʾ�������UIԤ����
    public RectTransform canvasRectTransform;  // ������RectTransform�����ڶ�λUIԪ��

    //adjustWidth �� adjustHeight ͨ������Ϊ 1024 ����Ϊ����ģ������ͼ��ĳߴ�һ�¡�
    //���磬���Ŀ����ģ�ͣ��� YOLO����������ͼ��Ϊ 1024x1024 �ֱ��ʣ���ôΪ���� Unity ��������ȷ��ӳ�����ʾ�������
    //����ת��ʱ��Ҫ����ԭʼͼ��Ŀ�Ⱥ͸߶��� 1024����ȷ���˴��������굽���������׼ȷӳ�䣬��������ʾ��λ��ߴ粻�Ե����⡣
    public int adjustWidth = 1024;  // ������Ļ�׼��ȣ���������ת��
    public int adjustHeight = 1024;  // ������Ļ�׼�߶ȣ���������ת��
    private readonly List<GameObject> activeImages = new();  // ��ǰ��ʾ��UIԪ���б����ڹ��������

    // ���ű�����ʱ��ע���¼��������
    private void OnEnable()
    {
        networkingClient.OnMessageReceived += HandleMessage;  // ע����Ϣ�����¼��Ĵ������
        networkingClient.OnDisconnected += HandleDisconnection;  // ע��Ͽ������¼��Ĵ������
    }

    // ���ű�����ʱ��ȡ��ע���¼��������
    private void OnDisable()
    {
        networkingClient.OnMessageReceived -= HandleMessage;  // ȡ��ע����Ϣ�����¼��Ĵ������
        networkingClient.OnDisconnected -= HandleDisconnection;  // ȡ��ע��Ͽ������¼��Ĵ������
    }

    // ������յ�����Ϣ
    private void HandleMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Debug.LogError("Received an empty message.");  // ������յ�����ϢΪ�գ���¼����
            return;
        }
        try
        {
            // ��JSON��Ϣ����ΪDetectionWrapper����
            DetectionWrapper detectionWrapper = JsonUtility.FromJson<DetectionWrapper>(message);
            if (detectionWrapper != null && detectionWrapper.detections != null)
            {
                UpdateUI(detectionWrapper.detections);  // ����UI����ʾ�������ļ����
            }
            else
            {
                Debug.LogError("Parsed JSON is null or does not contain detections.");  // ����������Ϊ�ջ򲻰���������ݣ���¼����
            }
        }
        catch (ArgumentException e)
        {
            Debug.LogError("JSON parse error: " + e.Message);  // ���JSON����ʧ�ܣ���¼������Ϣ
        }
    }

    // ����UI����ʾ�����
    private void UpdateUI(List<Detection> detections)
    {
        ClearActiveImages();  // ���֮ǰ��UIԪ��

        foreach (var detection in detections)
        {
            CreateImage(detection);  // Ϊÿ�������󴴽��µ�UIԪ��
        }
    }

    // ������ʾ�������UIԪ��
    private void CreateImage(Detection detection)
    {
        GameObject imageGO = Instantiate(imagePrefab, canvasRectTransform);  // ʵ����UIԤ����
        RectTransform rectTransform = imageGO.GetComponent<RectTransform>();

        float x = detection.bbox[0];
        float y = detection.bbox[1];
        float x2 = detection.bbox[2];
        float y2 = detection.bbox[3];

        // ����߽��Ŀ�Ⱥ͸߶�
        float width = x2 - x;  // ����߽��Ŀ�ȣ����½ǵ�x�����ȥ���Ͻǵ�x����
        float height = y2 - y;  // ����߽��ĸ߶ȣ����½ǵ�y�����ȥ���Ͻǵ�y����

        // ��ȡ�����ĳߴ�
        Vector2 canvasSize = canvasRectTransform.sizeDelta;  // ��ȡ�����Ŀ�Ⱥ͸߶ȣ���λΪ��������ϵ��

        // ��ԭʼͼ���еı߽������ת��Ϊ��������ϵ�е�����
        float xPos = x / adjustWidth * canvasSize.x;  // ��ԭʼͼ���x�����һ����ӳ�䵽�����Ŀ�ȣ��õ��ڻ����ϵ�x����
        float yPos = y / adjustHeight * canvasSize.y;  // ��ԭʼͼ���y�����һ����ӳ�䵽�����ĸ߶ȣ��õ��ڻ����ϵ�y����

        // ���߽��Ŀ�Ⱥ͸߶�ת��Ϊ�����ϵĳߴ�
        float rectWidth = width / adjustWidth * canvasSize.x;  // ��ԭʼͼ���б߽��Ŀ�ȹ�һ����ӳ�䵽�����Ŀ�ȣ��õ��ڻ����ϵĿ��
        float rectHeight = height / adjustHeight * canvasSize.y;  // ��ԭʼͼ���б߽��ĸ߶ȹ�һ����ӳ�䵽�����ĸ߶ȣ��õ��ڻ����ϵĸ߶�


        // ����UIԪ�ص�ê��λ�ã�ʹ���뻭���еı߽��λ�ö�Ӧ
        // xPos �Ǳ߽�����Ͻǵ� x ���꣬canvasSize.y - yPos - rectHeight ���ڼ������Ͻǵ� y ����
        rectTransform.anchoredPosition = new Vector2(xPos, canvasSize.y - yPos - rectHeight);

        // ����UIԪ�صĿ�Ⱥ͸߶ȣ�ʹ�����⵽�ı߽���Сƥ��
        rectTransform.sizeDelta = new Vector2(rectWidth, rectHeight);

        // ��ȡTextMeshPro��������ü���������
        TextMeshProUGUI textMeshPro = imageGO.GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.text = detection.class_name;  // ��ʾ�������
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in the image prefab.");  // ���Ԥ������û��TextMeshPro�������¼����
        }

        activeImages.Add(imageGO);  // ���µ�UIԪ����ӵ���б���
    }

    // �����ǰ��ʾ��UIԪ��
    private void ClearActiveImages()
    {
        foreach (var image in activeImages)
        {
            Destroy(image);  // ����UIԪ��
        }
        activeImages.Clear();  // ��ջ�б�
    }

    // ����Ͽ������¼�
    private void HandleDisconnection()
    {
        ClearActiveImages();  // �Ͽ�����ʱ�������UIԪ��
    }
}
