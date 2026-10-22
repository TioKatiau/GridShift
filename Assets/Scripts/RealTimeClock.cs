using UnityEngine;
using TMPro;

public class RealTimeClock : MonoBehaviour
{
    [Header("Relógio")]
    [SerializeField] private TMP_Text clockText;

    [Header("Sol")]
    [SerializeField] private Light sun;

    [Header("Ciclo")]
    [SerializeField] private float duracaoCicloMinutos = 32f;

    [Header("Intensidade")]
    [SerializeField] private float intensidadeDia = 1.2f;
    [SerializeField] private float intensidadeNoite = 0.05f;

    private float tempoDoCiclo;

    private const string SAVE_KEY = "HorarioDoJogo";

    private void Start()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            tempoDoCiclo = PlayerPrefs.GetFloat(SAVE_KEY);
        }
        else
        {
            System.DateTime agora = System.DateTime.Now;

            float horaInicial =
                agora.Hour +
                agora.Minute / 60f +
                agora.Second / 3600f;

            float duracaoCicloSegundos =
                duracaoCicloMinutos * 60f;

            tempoDoCiclo =
                horaInicial / 24f *
                duracaoCicloSegundos;
        }

        AtualizarTudo();
    }

    private void Update()
    {
        float duracaoCicloSegundos =
            duracaoCicloMinutos * 60f;

        tempoDoCiclo += Time.deltaTime;

        if (tempoDoCiclo >= duracaoCicloSegundos)
        {
            tempoDoCiclo -= duracaoCicloSegundos;
        }

        AtualizarTudo();
    }

    private void AtualizarTudo()
    {
        float duracaoCicloSegundos =
            duracaoCicloMinutos * 60f;

        float progresso =
            tempoDoCiclo / duracaoCicloSegundos;

        float horaVirtual =
            progresso * 24f;

        AtualizarRelogio(horaVirtual);
        AtualizarSol(horaVirtual);
    }

    private void AtualizarRelogio(float hora)
    {
        int horas = Mathf.FloorToInt(hora);

        int minutos =
            Mathf.FloorToInt((hora - horas) * 60f);

        horas %= 24;

        clockText.text =
            horas.ToString("00") + ":" +
            minutos.ToString("00");
    }

    private void AtualizarSol(float hora)
    {
        if (sun == null)
            return;

        // Faz o mesmo Sol girar durante as 24 horas
        float rotacaoX =
            (hora / 24f) * 360f - 90f;

        sun.transform.rotation =
            Quaternion.Euler(
                rotacaoX,
                170f,
                0f
            );

        // Dia: 06:00 - 18:00
        if (hora >= 6f && hora <= 18f)
        {
            float progressoDia =
                Mathf.InverseLerp(
                    6f,
                    18f,
                    hora
                );

            // Suaviza amanhecer e pôr do sol
            float intensidade =
                Mathf.Sin(
                    progressoDia * Mathf.PI
                );

            sun.intensity =
                Mathf.Lerp(
                    intensidadeNoite,
                    intensidadeDia,
                    intensidade
                );
        }
        else
        {
            // Noite
            sun.intensity = intensidadeNoite;
        }
    }

    private void OnApplicationQuit()
    {
        SalvarHorario();
    }

    private void OnApplicationPause(bool pausado)
    {
        if (pausado)
        {
            SalvarHorario();
        }
    }

    private void SalvarHorario()
    {
        PlayerPrefs.SetFloat(
            SAVE_KEY,
            tempoDoCiclo
        );

        PlayerPrefs.Save();
    }
}