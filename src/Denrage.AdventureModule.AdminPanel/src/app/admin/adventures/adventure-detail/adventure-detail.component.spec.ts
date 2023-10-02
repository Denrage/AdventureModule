import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdventureDetailComponent } from './adventure-detail.component';

describe('AdventureDetailComponent', () => {
  let component: AdventureDetailComponent;
  let fixture: ComponentFixture<AdventureDetailComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AdventureDetailComponent]
    });
    fixture = TestBed.createComponent(AdventureDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
