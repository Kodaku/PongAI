using UnityEngine;
using System.IO;
public class Observation
{
    float m_episodeDuration;
    int m_numberOfSteps;
    float m_qMin1;
    float m_qMin2;
    float m_qMax1;
    float m_qMax2;
    float m_qValue1;
    float m_qValue2;
    int m_hitCount1;
    int m_hitCount2;
    bool m_hasPlayer1Scored;

    public float episodeDuration
    {
        get { return m_episodeDuration; }
        set { m_episodeDuration = value; }
    }
    public int numberOfSteps
    {
        get { return m_numberOfSteps; }
        set { m_numberOfSteps = value; }
    }
    public float qMin1
    {
        get { return m_qMin1; }
        set { m_qMin1 = value; }
    }
    public float qMin2
    {
        get { return m_qMin2; }
        set { m_qMin2 = value; }
    }
    public float qMax1
    {
        get { return m_qMax1; }
        set { m_qMax1 = value; }
    }

    public float qMax2
    {
        get { return m_qMax2; }
        set { m_qMax2 = value; }
    }
    public float qValue1
    {
        get { return m_qValue1; }
        set { m_qValue1 = value; }
    }
    public float qValue2
    {
        get { return m_qValue2; }
        set { m_qValue2 = value; }
    }
    public int hitCount1
    {
        get { return m_hitCount1; }
        set { m_hitCount1 = value; }
    }
    public int hitCount2
    {
        get { return m_hitCount2; }
        set { m_hitCount2 = value; }
    }
    public bool hasPlayer1Scored
    {
        get { return m_hasPlayer1Scored; }
        set { m_hasPlayer1Scored = value; }
    }

    public void SaveToFile(bool append = true)
    {
        string tsvPath = Application.dataPath + "/Resources/Observations.tsv";
        string tsvData = m_episodeDuration.ToString().Replace(",", ".") + "\t" + 
                        m_numberOfSteps + "\t" +
                        m_qMin1.ToString().Replace(",", ".") + "\t" +
                        m_qMin2.ToString().Replace(",", ".") + "\t" + 
                        m_qMax1.ToString().Replace(",", ".") + "\t" +
                        m_qMax2.ToString().Replace(",", ".") + "\t" +
                        m_qValue1.ToString().Replace(",",".") + "\t" +
                        m_qValue2.ToString().Replace(",",".") + "\t" +
                        m_hitCount1 + "\t" +
                        m_hitCount2 + "\t" +
                        (m_hitCount1 + m_hitCount2) + "\t" +
                        m_hasPlayer1Scored.ToString().ToUpper();
        StreamWriter tsvWriter = new StreamWriter(tsvPath, append);
        tsvWriter.WriteLine(tsvData);
        tsvWriter.Flush();
        tsvWriter.Close();
    }
}