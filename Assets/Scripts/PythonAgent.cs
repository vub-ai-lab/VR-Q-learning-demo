using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
