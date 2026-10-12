
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MarkerInfoAutoSetup : MonoBehaviour
{
    [Header("Arrastra aquí los cuatro grupos de marcadores")]
    public Transform[] markerGroups;

    [Header("Tarjeta de información")]
    public MarkerInfoCardController cardController;

    private class ZoneData
    {
        public string nombre;
        public string barrio;
        public string estado;
        public string evento;
        public string riesgo;
        public string descripcion;
        public string fuente;

        public ZoneData(
            string nombre,
            string barrio,
            string estado,
            string evento,
            string riesgo,
            string descripcion,
            string fuente)
        {
            this.nombre = nombre;
            this.barrio = barrio;
            this.estado = estado;
            this.evento = evento;
            this.riesgo = riesgo;
            this.descripcion = descripcion;
            this.fuente = fuente;
        }
    }

    private readonly Dictionary<string, ZoneData> datos =
        new Dictionary<string, ZoneData>();

    private void Awake()
    {
        CargarDatos();
    }

    private void Start()
    {
        ConfigurarMarcadores();
    }

    private void CargarDatos()
    {
        // SEQUÍA
        Agregar("Drought_Siloe", "Siloé", "Siloé", "Sequía",
            "Condiciones de sequía y deslizamientos", "Alto / Crítico",
            "Racionamiento hídrico por baja en el río Meléndez e inestabilidad del suelo en laderas de alta pendiente.",
            "Secretaría de Gestión del Riesgo de Cali y EMCALI");

        Agregar("Drought_Melendez", "Meléndez", "Meléndez", "Sequía",
            "Condiciones de sequía y deslizamientos", "Alto",
            "Sensible a desabastecimiento de agua por sequía en su fuente hídrica y a desprendimientos de tierra en ladera.",
            "Alcaldía de Cali y EMCALI");

        Agregar("Drought_LosChorros", "Los Chorros", "Los Chorros", "Sequía",
            "Condiciones de sequía", "Medio-Alto",
            "Bajas presiones y suspensiones de agua en época seca por encontrarse al final de la red de distribución de ladera.",
            "CVC y EMCALI");

        Agregar("Drought_Polvorines", "Polvorines", "Polvorines", "Sequía",
            "Condiciones de sequía", "Alto",
            "Pérdida de suministro continuo durante épocas estivales por su ubicación topográfica en ladera alta.",
            "Secretaría de Gestión del Riesgo de Cali");

        Agregar("Drought_BelisarioCaicedo", "Belisario Caicedo",
            "Belisario Caicedo", "Sequía", "Condiciones de sequía", "Medio",
            "Afectación periódica en la red de acueducto cuando disminuyen los caudales de los ríos tutelares del oeste.",
            "EMCALI (Planes de Contingencia Hídrica)");

        // INCENDIOS
        Agregar("Fire_TerronColorado", "Terrón Colorado", "Terrón Colorado",
            "Incendio", "Incendios forestales y deslizamientos", "Alto",
            "Quemas de vegetación de ladera propiciadas por viento y movimientos en masa en construcciones sobre taludes.",
            "Cuerpo de Bomberos Voluntarios de Cali y DAGMA");

        Agregar("Fire_VistaHermosa", "Vista Hermosa", "Vista Hermosa",
            "Incendio", "Incendios forestales", "Alto",
            "Extensión de matorrales secos propensos a la rápida propagación de fuego en épocas de altas temperaturas.",
            "Plan Distrital de Prevención de Incendios Forestales");

        Agregar("Fire_SectorCristoRey", "Sector Cristo Rey", "Sector Cristo Rey",
            "Incendio", "Incendios forestales", "Alto",
            "Zona de ladera con alta combustibilidad vegetal y vientos fuertes propensa a conflagraciones.",
            "Cuerpo de Bomberos Voluntarios de Cali");

        Agregar("Fire_AltoMenga", "Alto Menga", "Alto Menga",
            "Incendio", "Incendios forestales", "Alto",
            "Cobertura de bosque seco tropical expuesta a vientos del norte que aceleran el fuego en verano.",
            "Secretaría de Gestión del Riesgo de Cali");

        Agregar("Fire_LaBuitrera", "La Buitrera", "La Buitrera",
            "Incendio", "Incendios forestales", "Alto",
            "Zona de interfaz urbano-rural con alta carga de masa forestal vulnerable a la temporada seca.",
            "CVC (Corporación Autónoma Regional del Valle del Cauca)");

        Agregar("Fire_Pance", "Pance", "Pance", "Incendio",
            "Incendios forestales e inundaciones", "Alto",
            "Quemas forestales en ladera y avenidas torrenciales o crecientes súbitas por la cercanía al río Pance.",
            "CVC (Sistema de Alertas Tempranas) y Bomberos Cali");

        // DESLIZAMIENTOS
        Agregar("Landslide_PampasdelMirador", "Pampas del Mirador",
            "Pampas del Mirador", "Deslizamiento", "Deslizamientos", "Alto",
            "Pendientes pronunciadas e inestabilidad geológica propensas a fallas en invernal.",
            "Estudio de Zonificación de Riesgo - Alcaldía de Cali");

        Agregar("Landslide_Melendez", "Meléndez", "Meléndez",
            "Deslizamiento", "Condiciones de sequía y deslizamientos", "Alto",
            "Sensible a desabastecimiento de agua por sequía en su fuente hídrica y a desprendimientos de tierra en ladera.",
            "Alcaldía de Cali y EMCALI");

        Agregar("Landslide_Siloe", "Siloé", "Siloé",
            "Deslizamiento", "Condiciones de sequía y deslizamientos", "Alto / Crítico",
            "Racionamiento hídrico por baja en el río Meléndez e inestabilidad del suelo en laderas de alta pendiente.",
            "Secretaría de Gestión del Riesgo de Cali y EMCALI");

        Agregar("Landslide_TerronColorado", "Terrón Colorado",
            "Terrón Colorado", "Deslizamiento",
            "Incendios forestales y deslizamientos", "Alto",
            "Quemas de vegetación de ladera propiciadas por viento y movimientos en masa en construcciones sobre taludes.",
            "Cuerpo de Bomberos Voluntarios de Cali y DAGMA");

        Agregar("Landslide_Comuna19", "Comuna 19 (Sectores de Ladera)",
            "Comuna 19 (Tejares, Los Cerros)", "Deslizamiento",
            "Deslizamientos", "Medio-Alto",
            "Presencia de fallas locales y taludes expuestos sensibles a la saturación de agua.",
            "Plan de Ordenamiento Territorial (POT) de Cali");

        // INUNDACIONES
        Agregar("Flood_CiudadeladelRio", "Ciudadela del Río",
            "Ciudadela del Río", "Inundación", "Inundaciones", "Alto",
            "Afectación por acumulación de aguas lluvias y baja capacidad de drenaje por su topografía plana.",
            "Proyecto Plan Jarillón de Cali");

        Agregar("Flood_Juanchito", "Juanchito", "Juanchito",
            "Inundación", "Inundaciones", "Crítico",
            "Sector ribereño vulnerable al desbordamiento directo del río Cauca en temporadas de alta lluvia.",
            "CVC y Corporación Jarillón de Cali");

        Agregar("Flood_Vallado", "Vallado", "Vallado",
            "Inundación", "Inundaciones", "Alto",
            "Reposicionamiento de aguas pluviales y reflujo en canales por insuficiencia del sistema de bombeo.",
            "EMCALI (Dirección de Aguas Lluvias)");

        Agregar("Flood_ElPeñón", "El Peñón", "El Peñón",
            "Inundación", "Inundaciones", "Medio",
            "Crecientes súbitas del río Cali o anegamiento urbano por desbordamiento de colectores locales.",
            "Secretaría de Gestión del Riesgo de Cali");

        Agregar("Flood_ElPobladoII", "El Poblado II", "El Poblado II",
            "Inundación", "Inundaciones", "Alto",
            "Depresión plana propensa al represamiento de escorrentía superficial en aguaceros intensos.",
            "Plan Maestro de Alcantarillado de EMCALI");

        Agregar("Flood_Pance", "Pance", "Pance", "Inundación",
            "Incendios forestales e inundaciones", "Alto",
            "Quemas forestales en ladera y avenidas torrenciales o crecientes súbitas por la cercanía al río Pance.",
            "CVC (Sistema de Alertas Tempranas) y Bomberos Cali");
    }

    private void Agregar(
        string clave,
        string nombre,
        string barrio,
        string estado,
        string evento,
        string riesgo,
        string descripcion,
        string fuente)
    {
        datos[Normalizar(clave)] = new ZoneData(
            nombre, barrio, estado, evento, riesgo, descripcion, fuente);
    }

    private void ConfigurarMarcadores()
    {
        int configurados = 0;

        foreach (Transform grupo in markerGroups)
        {
            if (grupo == null)
            {
                Debug.LogWarning(
                    "Hay un grupo de marcadores sin asignar.");
                continue;
            }

            foreach (Transform elemento in
                     grupo.GetComponentsInChildren<Transform>(true))
            {
                if (elemento == grupo)
                    continue;

                string clave = Normalizar(elemento.name);

                if (!datos.TryGetValue(clave, out ZoneData info))
                    continue;

                Button boton = elemento.GetComponent<Button>();
                Image imagen = elemento.GetComponent<Image>();

                if (boton == null && imagen != null)
                    boton = elemento.gameObject.AddComponent<Button>();

                if (boton == null)
                {
                    Debug.LogWarning(
                        "El marcador " + elemento.name +
                        " necesita un Button y un Image en el mismo objeto.");
                    continue;
                }

                MarkerInfo marker =
                    elemento.GetComponent<MarkerInfo>();

                if (marker == null)
                    marker = elemento.gameObject.AddComponent<MarkerInfo>();

                marker.nombreLugar = info.nombre;
                marker.barrio = info.barrio;
                marker.estado = info.estado;
                marker.evento = info.evento;
                marker.nivelRiesgo = info.riesgo;
                marker.descripcion = info.descripcion;
                marker.fuente = info.fuente;
                marker.cardController = cardController;

                configurados++;
            }
        }

        Debug.Log(
            "MarkerInfoAutoSetup: " + configurados +
            " marcadores configurados automáticamente de 22.");
    }

    private string Normalizar(string texto)
    {
        string descompuesto = texto.Normalize(NormalizationForm.FormD);
        StringBuilder resultado = new StringBuilder();

        foreach (char c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) !=
                UnicodeCategory.NonSpacingMark)
            {
                if (char.IsLetterOrDigit(c))
                    resultado.Append(char.ToLowerInvariant(c));
            }
        }

        return resultado.ToString();
    }
}
