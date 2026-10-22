using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class ClimaReal : MonoBehaviour
{
    [Header("LOCALIZAÇÃO")]
    [SerializeField] private float latitude = -22.9068f;
    [SerializeField] private float longitude = -43.1729f;

    [Header("TEXTOS")]
    [SerializeField] private TMP_Text temperaturaText;
    [SerializeField] private TMP_Text climaText;

    [Header("IMAGENS")]
    [SerializeField] private Image imagemClima;
    [SerializeField] private Sprite solSprite;
    [SerializeField] private Sprite luaSprite;

    [Header("ATUALIZAÇÃO")]
    [SerializeField] private float intervaloAtualizacao = 300f;

    private void Start()
    {
        Debug.Log("🔥 CLIMAMANAGER INICIOU!");

        StartCoroutine(AtualizarClimaPeriodicamente());
    }

    private IEnumerator AtualizarClimaPeriodicamente()
    {
        while (true)
        {
            Debug.Log("Buscando clima real...");

            yield return StartCoroutine(BuscarClima());

            Debug.Log("Próxima atualização em " + intervaloAtualizacao + " segundos.");

            yield return new WaitForSeconds(intervaloAtualizacao);
        }
    }

    private IEnumerator BuscarClima()
    {
        string url =
            "https://api.open-meteo.com/v1/forecast" +
            "?latitude=" + latitude +
            "&longitude=" + longitude +
            "&current=temperature_2m,weather_code,is_day" +
            "&timezone=auto";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("ERRO AO BUSCAR CLIMA: " + request.error);
                yield break;
            }

            string json = request.downloadHandler.text;

            Debug.Log("JSON recebido da API:");
            Debug.Log(json);

            DadosClima dados = JsonUtility.FromJson<DadosClima>(json);

            if (dados == null || dados.current == null)
            {
                Debug.LogError("Não foi possível interpretar os dados do clima.");
                yield break;
            }

            AtualizarInterface(dados);
        }
    }

    private void AtualizarInterface(DadosClima dados)
    {
        // =========================
        // TEMPERATURA
        // =========================

        int temperatura = Mathf.RoundToInt(
            dados.current.temperature_2m
        );

        temperaturaText.text = temperatura + "°C";


        // =========================
        // CONDIÇÃO DO TEMPO
        // =========================

        climaText.text = TraduzirClima(
            dados.current.weather_code
        );


        // =========================
        // SOL OU LUA
        // =========================

        if (dados.current.is_day == 1)
        {
            imagemClima.sprite = solSprite;

            Debug.Log("É DIA → mostrando SOL");
        }
        else
        {
            imagemClima.sprite = luaSprite;

            Debug.Log("É NOITE → mostrando LUA");
        }


        // =========================
        // DEBUG
        // =========================

        Debug.Log(
            "Temperatura: " + temperatura + "°C"
        );

        Debug.Log(
            "Código do clima: " + dados.current.weather_code
        );

        Debug.Log(
            "É dia? " + dados.current.is_day
        );
    }


    private string TraduzirClima(int codigo)
    {
        switch (codigo)
        {
            case 0:
                return "Ensolarado";

            case 1:
                return "Principalmente ensolarado";

            case 2:
                return "Parcialmente nublado";

            case 3:
                return "Nublado";

            case 45:
            case 48:
                return "Neblina";

            case 51:
            case 53:
            case 55:
                return "Chuvisco";

            case 56:
            case 57:
                return "Chuvisco congelante";

            case 61:
            case 63:
            case 65:
                return "Chuva";

            case 66:
            case 67:
                return "Chuva congelante";

            case 71:
            case 73:
            case 75:
            case 77:
                return "Neve";

            case 80:
            case 81:
            case 82:
                return "Pancadas de chuva";

            case 95:
            case 96:
            case 99:
                return "Tempestade";

            default:
                return "Clima desconhecido";
        }
    }


    [System.Serializable]
    private class DadosClima
    {
        public CurrentWeather current;
    }


    [System.Serializable]
    private class CurrentWeather
    {
        public float temperature_2m;
        public int weather_code;
        public int is_day;
    }
}