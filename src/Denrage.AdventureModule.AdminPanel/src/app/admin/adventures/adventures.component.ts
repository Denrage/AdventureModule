import { Component, OnInit } from '@angular/core';
import { Adventure } from 'src/app/models/adventure';
import { AdventureService } from 'src/app/services/adventure.service';

@Component({
  selector: 'app-adventures',
  templateUrl: './adventures.component.html',
  styleUrls: ['./adventures.component.scss']
})
export class AdventuresComponent implements OnInit {
  items: Adventure[] = [];

  constructor(private adventureService: AdventureService) {}
  
  ngOnInit(): void {
    this.items = this.adventureService.getAdventures();
  }
}
