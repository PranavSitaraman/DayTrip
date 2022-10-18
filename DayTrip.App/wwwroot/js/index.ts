import DotNetObject = DotNet.DotNetObject;
import Marker = google.maps.Marker;

export interface PinPreview {
    id: string;
    author: string;
    title: string;
    lat: number;
    lon: number;
    position: Position;
    kind: PinKind;
    kindId: string | null;
    expires: string | null;
    status: PinStatus;
}

enum PinKind {
    Warning,
    Blog,
    LostItem,
    Event
}

interface Position {
    lat: number;
    lon: number;
}

enum PinStatus {
    Active,
    Resolved,
    Expired
}


let map, meMarker;
let pinMarkers: Record<string, google.maps.Marker> = {}
let pins: Record<string, PinPreview> = {}
let mousedUp: boolean;
let mapNetObject: DotNetObject;
let meIcon: google.maps.Icon;
let markerIcons: Record<PinKind, google.maps.Icon>
let PinPopup;
const meSize = 40
const pinIconSize = 30


function getLocation(netObject: DotNetObject):
    boolean {

    async function returnPosition(position: GeolocationPosition) {
        console.log("reutrn positoin")
        console.log(position.coords.latitude, position.coords.longitude)
        await netObject.invokeMethodAsync('SetLocation', position.coords.latitude, position.coords.longitude);
    }


    function positionError(error: GeolocationPositionError) {
        console.error(error);
    }

    if (navigator.geolocation) {
        navigator.geolocation.watchPosition(returnPosition, positionError);
        return true;
    } else {
        return false;
    }
}

function initMap(netObject: DotNetObject, elementId: string, lat: number, lon: number, zoom: number) {
    initResources()
    mapNetObject = netObject;
    let latLng = new google.maps.LatLng(lat, lon);
    let options: google.maps.MapOptions = {
        zoom: zoom, center: latLng,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        streetViewControl: false,
        scaleControl: false,
        zoomControl: false,
        mapTypeControl: false,
        fullscreenControl: false,
        clickableIcons: false
    };

    console.log(elementId)
    let mapElement = document.getElementById(elementId);
    map = new google.maps.Map(mapElement, options);
    pinMarkers = {};
    pins = {};


    mousedUp = false;
    map.addListener('mousedown', (event) => {
        mousedUp = false;
        setTimeout(async () => {
            if (mousedUp === false) {
                let lat = event.latLng.lat();
                let lon = event.latLng.lng();
                await netObject.invokeMethodAsync("OnLongPress", lat, lon);
            }
        }, 500);
    });

    map.addListener('mouseup', () => mousedUp = true);

    map.addListener('dragstart', () => mousedUp = true);
}

function initResources() {
    meIcon = {
        url: "/assets/icons/man.svg",
        scaledSize: new google.maps.Size(meSize, meSize),
        origin: new google.maps.Point(0, 0),
        anchor: new google.maps.Point(meSize / 2, meSize / 2)
    };
    markerIcons = {
        [PinKind.Warning]: {
            url: '/assets/icons/warning.svg',
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        },

        [PinKind.LostItem]: {
            url: '/assets/icons/help.svg',
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        },


        [PinKind.Blog]: {
            url: "/assets/icons/newspaper.svg",
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)

        },

        [PinKind.Event]: {
            url: "/assets/icons/calendar-clear.svg",
            scaledSize: new google.maps.Size(pinIconSize, pinIconSize),
            origin: new google.maps.Point(0, 0),
            anchor: new google.maps.Point(pinIconSize / 2, pinIconSize / 2)
        }
    };

    PinPopup = class extends google.maps.OverlayView {
        markerMap: google.maps.Map | google.maps.StreetViewPanorama
        marker: google.maps.Marker
        position: google.maps.LatLng;
        containerDiv: HTMLDivElement;

        constructor(marker: google.maps.Marker, content: HTMLElement) {
            super();
            this.marker = marker;
            this.position = marker.getPosition();

            let closeContainer = document.createElement('div');
            closeContainer.classList.add("popup-bubble");
            const closeIcon = document.createElement("ion-icon");
            closeIcon["name"] = "close";
            closeIcon.onclick = () => {
                this.setMap(null);
            };
            closeContainer.appendChild(content);
            closeContainer.appendChild(closeIcon);

            // This zero-height div is positioned at the bottom of the bubble.
            const bubbleAnchor = document.createElement("div");

            bubbleAnchor.classList.add("popup-bubble-anchor");
            bubbleAnchor.appendChild(closeContainer);

            // This zero-height div is positioned at the bottom of the tip.
            this.containerDiv = document.createElement("div");
            this.containerDiv.classList.add("popup-container");
            this.containerDiv.appendChild(bubbleAnchor);

            PinPopup.preventMapHitsAndGesturesFrom(this.containerDiv)
        }

        /** Called when the popup is added to the map. */
        onAdd() {
            this.getPanes()!.floatPane.appendChild(this.containerDiv);
            this.markerMap = this.marker.getMap()
            this.marker.setMap(null)
        }


        /** Called when the popup is removed from the map. */
        onRemove() {
            if (this.containerDiv.parentElement) {
                this.containerDiv.parentElement.removeChild(this.containerDiv);
            }
            this.marker.setMap(this.markerMap)
        }

        /** Called each frame when the popup needs to draw itself. */
        draw() {
            const divPosition = this.getProjection().fromLatLngToDivPixel(
                this.position
            )!;

            // Hide the popup when it is far out of view.
            const display = "block";

            if (display === "block") {
                this.containerDiv.style.left = divPosition.x + "px";
                this.containerDiv.style.top = divPosition.y + "px";
            }

            if (this.containerDiv.style.display !== display) {
                this.containerDiv.style.display = display;
            }
        }

    }
}


function moveMeMarker(lat: number, lon: number) {
    let pos = {lat: lat, lng: lon};
    if (meMarker == undefined) {
        meMarker = new google.maps.Marker({
            position: pos,
            map: map,
            icon: meIcon
        })
    } else {
        meMarker.setPosition(pos);
    }
}

let curInfoWindow;
let curInfoWindowId;

function previewPin(id: string) {

    let pin: PinPreview = pins[id];
    console.log(pin)
    let marker: google.maps.Marker = pinMarkers[id];
    let content = document.createElement("div")
    content.className = "google-map-info";
    content.innerHTML = `<h2>${pin.title}</h2>`;

    content.onclick = () => pinDetails(id);

    curInfoWindowId = id;
    curInfoWindow = new PinPopup(
        marker,
        content
    )
    curInfoWindow.setMap(map);
}


function placePin(pin: PinPreview) {
    console.log(pins)
    console.log("Placed pin")
    console.log(pin)
    pins[pin.id] = pin;
    let marker = new google.maps.Marker({
        position: {lat: pin.lat, lng: pin.lon},
        map: map,
        icon: markerIcons[pin.kind]

    });

    marker.addListener("click", () => previewPin(pin.id));

    pinMarkers[pin.id] = marker;
}

function pinDetails(id: string) {
    mapNetObject.invokeMethodAsync('ViewDetails', id);
}


let longPressIndicator: google.maps.Marker;

function placeLongPressIndicator(lat: number, lon: number) {
    let pos = {lat: lat, lng: lon};
    if (longPressIndicator == undefined) {
        longPressIndicator = new google.maps.Marker({
            position: pos,
            map: map,
            icon: {
                url: "/assets/icons/pin.svg",
                scaledSize: new google.maps.Size(meSize, meSize),
                origin: new google.maps.Point(0, 0),
                anchor: new google.maps.Point(meSize / 2, meSize),
                fillColor: "red"
            }
        })
    } else {
        longPressIndicator.setPosition(pos);
    }
}

function removeLongPressIndicator() {
    if (longPressIndicator != undefined) {
        longPressIndicator.setMap(null);
        longPressIndicator = undefined;
    }
}
