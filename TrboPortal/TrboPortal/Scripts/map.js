import * as api from './api.js';
import * as mapsHelper from './maps-helper.js';
import MapProperties from './maps-helper.js';
// Initial data load.
jQuery(document).ready(function (_$) {
    console.log("Init.");
    RefreshMap();
});


/**
 * Perform GET request to get values from server
 * */
function RefreshMap() {
    api.performApiCall("gps/history", 'GET', null,
        (response) => response.json().then(json => {
            gpsMeasurementsFormatter(json);
            $('#preloader').hide();
        }),
        (response) => Promise.resolve(response.text())
            .then(JSON.parse)
            .then(resp_data => showApiErrorWarning("Unable to load data", api.getValidationErrors(resp_data)))
    );
}

function processGetResponse(json) {

}

/**
 * Renders part of DetailFormatter's view for expaned row containing table of most recent GPS measurements of said row.
 * @param {any} gpsMeasurements
 */
function gpsMeasurementsFormatter(gpsMeasurements) {
    console.log(gpsMeasurements)
    var html = []
    if (gpsMeasurements == null || gpsMeasurements.length == 0) {
        console.log("Received no GPSMeasurements, not able to draw map.");
        return html;
    }

    const mapProperties = new MapProperties("Map", gpsMeasurements);
    mapProperties.renderMarkers = false;
    mapProperties.renderLines = false;
    mapProperties.renderHeatmap = true;

    mapsHelper.showMap(mapProperties);

    // Call renderRadioGpsMap after 100ms as this HTML needs to be added first before map div can be found.
    //setTimeout(function () { renderRadioGpsMap(gpsMeasurements); }, 100);

    return html.join('\r\n');
}

/**
 * Renders Google Maps drawing a line between GPS points using PolyLine.
 * Assumed coordinates are ordered descending in time (newest on top).
 * Map is centered on newest GPS coordinate.
 * @param {any} divId Div id to render map in
 * @param {any} coordinates Coordinates to render.
 */
function renderRadioGpsMap(gpsMeasurements) {
    const divId = "Map"
    const centerLat = gpsMeasurements[0].Latitude
    const centerLong = gpsMeasurements[0].Longitude
    console.log(`Rendering RadioGpsMap for div ${divId}, centering on ${centerLat}, ${centerLong}`)
    const map = new google.maps.Map(document.getElementById(divId), {
        zoom: 15,
        center: { lat: centerLat, lng: centerLong  },
        mapTypeId: "terrain",
    });
    //polyLineCoordinatesLine.setMap(map);
    //renderMapMarkers(map, mapsCoordinates);
    renderHeatmap(map, gpsMeasurements);
}

function renderHeatmap(map, mapsCoordinates) {
    const heatMapData = [];
    for (let i = 0; i < mapsCoordinates.length; i++) {
        const measurement = mapsCoordinates[i];
        heatMapData.push({
            location: new google.maps.LatLng(measurement.Latitude, measurement.Longitude), weight: measurement.Rssi
        });
    }
    var heatmap = new google.maps.visualization.HeatmapLayer({
        data: heatMapData
    });
    heatmap.setMap(map);


}

/**
 * Adds red markers with hover-over with GPS timestamp on Google Maps.
 * @param {any} map Map to render markers on
 * @param {any} mapsCoordinates The markers to render.
 */
function renderMapMarkers(map, mapsCoordinates) {
    for (let i = 0; i < mapsCoordinates.length; i++) {
        const measurement = mapsCoordinates[i];

        new google.maps.Marker({
            position: { lat: measurement.lat, lng: measurement.lng },
            map,
            title: measurement.ts,
        });
    }
}