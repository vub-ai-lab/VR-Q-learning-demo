using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PythonAgent : MonoBehaviour
{
    private System.Diagnostics.Process process;

    private void Awake()
    {
        // Starts python server
        process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "python";
        process.StartInfo.Arguments = "server.py";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

        process.Start();
		Debug.Log ("Started python server");

        // Import library since that we need only once anyway
        sendMessage("import AIToolbox");
    }

    public void OnApplicationQuit()
    {
        process.Kill();
    }

    public void initializeQLearning(int state, int action) {
        sendMessage("q = AIToolbox.MDP.QLearning(" + state + "," + action + ")");
        sendMessage("q.setLearningRate(0.4)");
        sendMessage("q.setDiscount(0.9)");
    }

    public void UpdateQTable(int state, int action, int nextState, float reward) {
        sendMessage("q.stepUpdateQ(" + state + "," + action + "," + nextState + "," + reward + ")");
    }
    public float GetQval(int state, int action)
    {
        var response = sendMessageWithResponse("q.getQFunction()[" + state + "," + action + "]");
        Debug.Log(response);
        return float.Parse(response);
    }

    public string sendMessageWithResponse(string msg)
    {
        msg = "print(" + msg + ")";
        return sendMessage(msg);
    }

    public string sendMessage(string msg)
    {
        var str = "http://localhost:8080/";
        str += msg.Replace(' ', '_');
        Debug.Log("Sending request: " + str);

        var www = UnityWebRequest.Get(str);
        www.SendWebRequest();

        while (!www.isDone) { };
        if (www.isNetworkError)
        {
            Debug.Log(www.error);
            return "";
        }

        var retval = www.downloadHandler.text;

        if (retval.StartsWith("error"))
        {
            Debug.Log("Error for message: " + msg + "\n" + retval);
        }

        return retval;
    }
}

/*
// Old behaviour with embedded process
public class PythonAgent : MonoBehaviour {

    private System.Diagnostics.Process process;
    private System.IO.StreamWriter input;
    private System.IO.StreamReader output;
    private string outbuffer;

    // Use this for initialization
    void Start () {
        outbuffer = "";

        process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "python";
        process.StartInfo.Arguments = "-i";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;

        process.Start();

        input = process.StandardInput;
        output = process.StandardOutput;

        // First input for some reason is corrupted so we flush.
        input.WriteLine(@"");
        input.WriteLine(@"");
        input.WriteLine(@"");

        sendMessage(@"import AIToolbox");
    }

    public void initializeQLearning(int state, int action)
    {
        sendMessage(@"q = AIToolbox.MDP.QLearning(" + state + "," + action + ")");
        sendMessage(@"q.setLearningRate(0.4)");
        sendMessage(@"q.setDiscount(0.9)");
    }

    public void UpdateQTable(int state, int action, int nextState, float reward)
    {
        sendMessage(@"q.stepUpdateQ(" + state + "," + action + "," + nextState + "," + reward + ")");
    }
    public float GetQval(int state, int action)
    {
        var response = sendMessageWithResponse(@"q.getQFunction()[" + state + "," + action + "]");
        Debug.Log(response);
        return float.Parse(response);
    }

    public void sendMessage(string message)
    {
        input.WriteLine(message);
    }

    public string sendMessageWithResponse(string message)
    {
        input.WriteLine(message);
        int c;
        while ((c = output.Peek()) >= 0)
        {
            output.Read();
            if (c == '\n')
            {
                var retval = outbuffer;
                outbuffer = "";
                return retval;
            }
            outbuffer += (char)c;
        }
        return null;
    }

    public void OnApplicationQuit()
    {
        process.Kill();
    }
}
*/
