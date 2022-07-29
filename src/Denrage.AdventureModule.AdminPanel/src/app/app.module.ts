import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './app.component';
import { PlayerComponent } from './player/player.component';
import { AdventureGroupComponent } from './adventure-group/adventure-group.component';
import { AdventureComponent } from './adventure/adventure.component';
import { AdventureListComponent } from './adventure-list/adventure-list.component';
import { PlayerListComponent } from './player-list/player-list.component';
import { EditPlayerDialogComponent } from './edit-player-dialog/edit-player-dialog.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule } from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';

@NgModule({
  declarations: [
    AppComponent,
    PlayerComponent,
    AdventureGroupComponent,
    AdventureComponent,
    AdventureListComponent,
    PlayerListComponent,
    EditPlayerDialogComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    MatCardModule,
    MatButtonModule,
    MatListModule,
    MatGridListModule,
    MatIconModule,
    MatSidenavModule,
    MatToolbarModule,
    FlexLayoutModule,
    MatDividerModule,
    MatFormFieldModule,
    MatDialogModule,
    MatInputModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: AdventureListComponent },
      { path: 'adventures', component: AdventureListComponent },
      { path: 'adventures/:id', component: AdventureComponent },
      { path: 'players', component: PlayerListComponent }
    ])
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
