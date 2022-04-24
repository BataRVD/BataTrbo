import * as api from './api.js';

export default  class MapProperties {
    // div ID containing map element
    divId;
    // GPS points to render
    gpsMeasurements;
    // Render options
    renderMarkers = true;
    renderLines = false;
    renderHeatmap = false;
    center = { lat: 0, lng: 0 };
    zoom = 15;

    // Delay (ms) before JavaScript for rendering is invoked.
    renderDelay = 100;

    constructor(divId, gpsMeasurements) {
        this.gpsMeasurements = gpsMeasurements;
        this.divId = divId;
        if (gpsMeasurements != null && gpsMeasurements.length != 0) {
            this.center = { lat: gpsMeasurements[0].Latitude, lng: gpsMeasurements[0].Longitude };
        }
    }
}

/**
 * Renders part view for expaned row containing table of most recent GPS measurements of said row.
 * @param {any} gpsMeasurements
 */
export function showMap(mapProperties) {
    var html = []
    if (mapProperties.gpsMeasurements == null || mapProperties.gpsMeasurements.length == 0) {
        console.log("Received no GPSMeasurements, not able to draw map.");
        return html;
    }
    // Call renderRadioGpsMap after 'x' ms, as this HTML needs to be added first before map div can be found.
    setTimeout(function () { renderMeasurements(mapProperties); }, mapProperties.renderDelay);
    return html.join('\r\n');
}

/**
 * Renders Google Maps drawing a line between GPS points using PolyLine.
 * Assumed coordinates are ordered descending in time (newest on top).
 * Map is centered on newest GPS coordinate.
 * @param {any} divId Div id to render map in
 * @param {any} coordinates Coordinates to render.
 */
function renderMeasurements(mapProperties) {
    console.log(`Rendering Map with ${mapProperties.gpsMeasurements.length} points for div ${mapProperties.divId}, centering on ${mapProperties.center.lat},${mapProperties.center.lng}...`)
    const map = new google.maps.Map(document.getElementById(mapProperties.divId), {
        zoom: mapProperties.zoom,
        center: mapProperties.center,
        mapTypeId: "terrain",
    });

    if (mapProperties.renderLines) {
        renderLines(map, mapProperties);
    }
    if (mapProperties.renderMarkers) {
        renderMapMarkers(map, mapProperties);
    }
    if (mapProperties.renderHeatmap) {
        renderHeatmap(map, mapProperties);
    }
}

function renderLines(map, mapProperties) {
    console.log("Rendering lines...");
    const mapsCoordinates = [];
    $.each(mapProperties.gpsMeasurements, function (index, m) {
        mapsCoordinates.push({ ts: moment(m['Timestamp']).format("yyyy-MM-DD HH:mm:ss"), lat: m.Latitude, lng: m.Longitude})
    })

    const polyLineCoordinatesLine = new google.maps.Polyline({
        path: mapsCoordinates,
        geodesic: true,
        strokeColor: "#FF0000",
        strokeOpacity: 1.0,
        strokeWeight: 2,
    });
    polyLineCoordinatesLine.setMap(map);
}


/**
 * Adds red markers with hover-over with GPS timestamp on Google Maps.
 * @param {any} map Map to render markers on
 * @param {any} mapsCoordinates The markers to render.
 */
function renderMapMarkers(map, mapProperties) {
    console.log("Rendering markers...");
    $.each(mapProperties.gpsMeasurements, function (index, m) {
        new google.maps.Marker({
            position: { lat: m.Latitude, lng: m.Longitude },
            map,
            title: `Radio:\t${m.RadioID}\r\nTime:\t${moment(m['Timestamp']).format("yyyy-MM-DD HH:mm:ss")}\r\nRSSI:\t${m.Rssi}`,
        });
    })
}

function renderHeatmap(map, mapProperties) {
    console.log("Rendering heatmap...");
    const heatMapData = [];
    $.each(mapProperties.gpsMeasurements, function (index, m) {
        heatMapData.push({ location: new google.maps.LatLng(m.Latitude, m.Longitude), weight: Math.abs(m.Rssi)});
    })

    var defaultGradient = ["rgba(102, 255, 0, 0)",
        "rgba(102, 255, 0, 1)",
        "rgba(147, 255, 0, 1)",
        "rgba(193, 255, 0, 1)",
        "rgba(238, 255, 0, 1)",
        "rgba(244, 227, 0, 1)",
        "rgba(249, 198, 0, 1)",
        "rgba(255, 170, 0, 1)",
        "rgba(255, 113, 0, 1)",
        "rgba(255, 57, 0, 1)",
        "rgba(255, 0, 0, 1)"]


    var heatmap = new google.maps.visualization.HeatmapLayer({
        data: heatMapData,
        map: map,
        radius: 20,
        gradient: defaultGradient
    });
    
}
