using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class NetworkingClient : MonoBehaviour
{
    // ��������IP��ַ�Ͷ˿ں�
    public string host = "192.168.68.102";
    public int port = 8080;

    // TCP�ͻ��˺�������
    private TcpClient tcpClient;
    private NetworkStream networkStream;

    // �Ƿ����ڳ������ӵı�־
    private bool attemptingConnection = false;

    // ���ڶ�ȡ���ݵ�Э��
    private Coroutine readDataCoroutine;

    // ί�к��¼������ڴ�����Ϣ���պͶϿ�����
    public delegate void MessageReceivedHandler(string message);
    public event MessageReceivedHandler OnMessageReceived;
    public delegate void DisconnectedHandler();
    public event DisconnectedHandler OnDisconnected;

    // UIԪ��������ʾ����״̬
    [SerializeField] private Image connectionStatusImage;
    [SerializeField] private Color connectedColor = Color.green;  // ����ʱ����ɫ
    [SerializeField] private Color disconnectedColor = Color.red; // �Ͽ�ʱ����ɫ

    // ������ʱ��ʼ������״̬Ϊ�Ͽ�״̬
    void Start()
    {
        UpdateConnectionStatus(false);
    }

    // �첽���ӵ��������ķ���
    public async void Connect()
    {
        // ������ڳ������ӻ��Ѿ����ӣ��򲻽����µ�����
        if (attemptingConnection || (tcpClient != null && tcpClient.Connected)) return;
        attemptingConnection = true;

        try
        {
            tcpClient = new TcpClient();  // �����µ�TCP�ͻ���
            await tcpClient.ConnectAsync(host, port);  // �첽���ӵ�������
            networkStream = tcpClient.GetStream();  // ��ȡ������
            UpdateConnectionStatus(true);  // ��������״̬Ϊ������
            readDataCoroutine ??= StartCoroutine(ReadDataAsync());  // ��ʼ��ȡ���ݵ�Э��
        }
        catch (Exception e)
        {
            // ����ʧ��ʱ��ӡ������Ϣ����������״̬Ϊ�Ͽ�
            Debug.LogError("Failed to connect to the TCP server: " + e.Message);
            UpdateConnectionStatus(false);
        }
        finally
        {
            attemptingConnection = false;  // �������ӳ���
        }
    }

    // �Ͽ��������������
    public void Disconnect()
    {
        if (tcpClient != null)
        {
            if (tcpClient.Connected)
            {
                networkStream.Close();  // �ر�������
                tcpClient.Close();  // �ر�TCP�ͻ���
                UpdateConnectionStatus(false);  // ��������״̬Ϊ�Ͽ�
                OnDisconnected?.Invoke();  // �����Ͽ������¼�
            }

            tcpClient = null;  // �ͷ�TCP�ͻ�����Դ
            networkStream = null;  // �ͷ���������Դ

            if (readDataCoroutine != null)
            {
                StopCoroutine(readDataCoroutine);  // ֹͣ��ȡ���ݵ�Э��
                readDataCoroutine = null;  // �ͷ�Э����Դ
            }
        }
    }

    // ���·������������ķ���
    public void UpdateHost(string newHost)
    {
        host = newHost;  // ����������
    }

    // ��������״̬��UI��ʾ
    private void UpdateConnectionStatus(bool isConnected)
    {
        if (connectionStatusImage != null)
        {
            // ��������״̬����UIͼ�����ɫ
            connectionStatusImage.color = isConnected ? connectedColor : disconnectedColor;
        }
    }

    // Э�̣������첽��ȡ���������͵�����
    IEnumerator ReadDataAsync()
    {
        byte[] buffer = new byte[4096];  // ���ڴ洢���յ����ݵĻ�����

        while (true)
        {
            if (tcpClient != null && tcpClient.Connected && networkStream != null)
            {
                if (networkStream.DataAvailable)  // ����Ƿ������ݿ���
                {
                    Task<int> readTask = networkStream.ReadAsync(buffer, 0, buffer.Length);  // �첽��ȡ����
                    yield return new WaitUntil(() => readTask.IsCompleted);  // �ȴ���ȡ���

                    if (readTask.Exception == null)
                    {
                        int bytesRead = readTask.Result;  // ��ȡ��ȡ�������ֽ���
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);  // ���ֽ�����ת��Ϊ�ַ���
                        if (!string.IsNullOrEmpty(message))
                        {
                            OnMessageReceived?.Invoke(message);  // ������Ϣ�����¼�
                        }
                    }
                    else
                    {
                        // �����ȡ�����з����쳣����ӡ������Ϣ
                        Debug.LogError(readTask.Exception.ToString());
                    }
                }
            }
            yield return null;  // �ȴ���һ֡
        }
    }

    // ��Ӧ�ó����˳�ʱ�Ͽ�����
    void OnApplicationQuit()
    {
        Disconnect();  // �Ͽ��������������
    }
}
