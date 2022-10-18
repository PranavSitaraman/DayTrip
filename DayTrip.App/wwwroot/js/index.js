var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var PinKind;
(function (PinKind) {
    PinKind[PinKind["Warning"] = 0] = "Warning";
    PinKind[PinKind["Blog"] = 1] = "Blog";
    PinKind[PinKind["LostItem"] = 2] = "LostItem";
    PinKind[PinKind["Event"] = 3] = "Event";
})(PinKind || (PinKind = {}));
var PinStatus;
(function (PinStatus) {
    PinStatus[PinStatus["Active"] = 0] = "Active";
    PinStatus[PinStatus["Resolved"] = 1] = "Resolved";
    PinStatus[PinStatus["Expired"] = 2] = "Expired";
})(PinStatus || (PinStatus = {}));
let map, meMarker;
let pinMarkers = {};
let pins = {};
let mousedUp;
let mapNetObject;
let meIcon;
let markerIcons;
let PinPopup;
const meSize = 40;
const pinIconSize = 30;
function getLocation(netObject) {
    function returnPosition(position) {
        return __awaiter(this, void 0, void 0, function* () {
            console.log("reutrn positoin");
            console.log(position.coords.latitude, position.coords.longitude);
            yield netObject.invokeMethodAsync('SetLocation', position.coords.latitude, position.coords.longitude);
        });
    }
    function positionError(error) {
        console.error(error);
    }
    if (navigator.geolocation) {
        navigator.geolocation.watchPosition(returnPosition, positionError);
        return true;
    }
    else {
        return false;
    }
}
function initMap(netObject, elementId, lat, lon, zoom) {
    initResources();
    mapNetObject = netObject;
    let latLng = new google.maps.LatLng(lat, lon);
    let options = {
        zoom: zoom, center: latLng,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        streetViewControl: false,
        scaleControl: false,
        zoomControl: false,
        mapTypeControl: false,
        fullscreenControl: false,
        clickableIcons: false
    };
    console.log(elementId);
    let mapElement = document.getElementById(elementId);
    map = new google.maps.Map(mapElement, options);
    pinMarkers = {};
    pins = {};
    mousedUp = false;
    map.addListener('mousedown', (event) => {
        mousedUp = false;
        setTimeout(() => __awaiter(this, void 0, void 0, function* () {
            if (mousedUp === false) {
                let lat = event.latLng.lat();
                let lon = event.latLng.lng();
                yield netObject.invokeMethodAsync("OnLongPress", lat, lon);
            }
        }), 500);
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
        constructor(marker, content) {
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
            PinPopup.preventMapHitsAndGesturesFrom(this.containerDiv);
        }
        /** Called when the popup is added to the map. */
        onAdd() {
            this.getPanes().floatPane.appendChild(this.containerDiv);
            this.markerMap = this.marker.getMap();
            this.marker.setMap(null);
        }
        /** Called when the popup is removed from the map. */
        onRemove() {
            if (this.containerDiv.parentElement) {
                this.containerDiv.parentElement.removeChild(this.containerDiv);
            }
            this.marker.setMap(this.markerMap);
        }
        /** Called each frame when the popup needs to draw itself. */
        draw() {
            const divPosition = this.getProjection().fromLatLngToDivPixel(this.position);
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
    };
}
function moveMeMarker(lat, lon) {
    let pos = { lat: lat, lng: lon };
    if (meMarker == undefined) {
        meMarker = new google.maps.Marker({
            position: pos,
            map: map,
            icon: meIcon
        });
    }
    else {
        meMarker.setPosition(pos);
    }
}
let curInfoWindow;
let curInfoWindowId;
function previewPin(id) {
    let pin = pins[id];
    console.log(pin);
    let marker = pinMarkers[id];
    let content = document.createElement("div");
    content.className = "google-map-info";
    content.innerHTML = `<h2>${pin.title}</h2>`;
    content.onclick = () => pinDetails(id);
    curInfoWindowId = id;
    curInfoWindow = new PinPopup(marker, content);
    curInfoWindow.setMap(map);
}
function placePin(pin) {
    console.log(pins);
    console.log("Placed pin");
    console.log(pin);
    pins[pin.id] = pin;
    let marker = new google.maps.Marker({
        position: { lat: pin.lat, lng: pin.lon },
        map: map,
        icon: markerIcons[pin.kind]
    });
    marker.addListener("click", () => previewPin(pin.id));
    pinMarkers[pin.id] = marker;
}
function pinDetails(id) {
    mapNetObject.invokeMethodAsync('ViewDetails', id);
}
let longPressIndicator;
function placeLongPressIndicator(lat, lon) {
    let pos = { lat: lat, lng: lon };
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
        });
    }
    else {
        longPressIndicator.setPosition(pos);
    }
}
function removeLongPressIndicator() {
    if (longPressIndicator != undefined) {
        longPressIndicator.setMap(null);
        longPressIndicator = undefined;
    }
}
//# sourceMappingURL=index.js.map