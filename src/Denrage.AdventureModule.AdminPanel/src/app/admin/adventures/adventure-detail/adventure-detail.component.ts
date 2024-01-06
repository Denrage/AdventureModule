import { Component, OnInit } from '@angular/core';
import { LatLngBounds, Map as LeafletMap, MapOptions, latLng, tileLayer, CRS, LatLng, marker, icon, MarkerOptions, Icon, Layer, divIcon, Marker } from 'leaflet';
import { TreeNode } from 'primeng/api';
import { Observable, delay, repeat, timer } from 'rxjs';
import { AdventureStep } from 'src/app/models/adventure';
import { Group } from 'src/app/models/group';
import { AdventureService } from 'src/app/services/adventure.service';

@Component({
  selector: 'app-adventure-detail',
  templateUrl: './adventure-detail.component.html',
  styleUrls: ['./adventure-detail.component.scss']
})
export class AdventureDetailComponent implements OnInit {
  public groups: TreeNode[] = [];
  public steps: any = [["", ""]];

  private marker: Map<string, Marker<any>> = new Map<string, Marker<any>>();

  constructor(public adventureService: AdventureService) { }

  ngOnInit(): void {
    let groups = this.adventureService.getAdventures()[0].groups;
    for (let i = 0; i < groups.length; i++) {
      const element = groups[i];
      let node: TreeNode = {
        key: i.toString(),
        label: element.name,
        children: [],
        style: `background-color: ${element.color}10;`
      };

      for (let j = 0; j < element.users.length; j++) {
        node.children?.push({
          key: `${i}-${j}`,
          label: element.users[j].ingameUser.accountName,
        })
      }

      this.groups.push(node);
    }
    this.steps = this.adventureService.getAdventures()[0].steps.map(x => [x.name, groups.find(y => y.step == x.name)?.name ?? ""]);
  }

  options: MapOptions = {
    layers: [
      tileLayer('https://tiles.gw2.io/1/1/{z}/{x}/{y}.jpg', { maxZoom: 7, noWrap: true, tileSize: 256, })
    ],
    maxZoom: 7,
    minZoom: 2,
    crs: CRS.Simple,
    center: latLng(0, 0),
    zoom: 0,
  }

  unproject(coord: any, map: any) {
    return map.unproject(coord, map.getMaxZoom());
  }

  onMapReady(map: LeafletMap) {
    map.setMaxBounds(new LatLngBounds(this.unproject([0, 0], map), this.unproject([81920, 114688], map)));
    map.setView(map.unproject([48000, 30720], map.getMaxZoom()), 4);

    this.adventureService.getAdventures()[0].groups.forEach(x => {
      x.users.forEach(y => {
        let mark = marker(map.unproject([y.ingameUser.positionX, y.ingameUser.positionY], map.getMaxZoom()), {
          icon: divIcon({
            className: "leaflet-player-marker",
            html: `<img style="border-color:${x.color};" src="${y.profilePicture}"/>`
          }),
          title: y.ingameUser.accountName,
        });

        mark.bindPopup(y.ingameUser.accountName);
        mark.addEventListener("click", e => mark.openPopup());
        mark.addTo(map);
        this.marker.set(y.ingameUser.accountName, mark);
      })
    });

    let observable = new Observable<LatLng>(subscriber => {
      let previousPosition = this.marker.get("honky.3864")?.getLatLng()!;
      let nextPosition = new LatLng(previousPosition.lat + 0.1, previousPosition.lng);
      subscriber.next(nextPosition);
      subscriber.complete();
    }).pipe(repeat({count: 20, delay:500})).subscribe(x => {
     this.marker.get("honky.3864")?.setLatLng(x);
     console.log("Set latlng: " + x);
    });
  }

}
