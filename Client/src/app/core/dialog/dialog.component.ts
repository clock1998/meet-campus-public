import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';

@Component({
    selector: 'app-dialog',
    imports: [],
    templateUrl: './dialog.component.html',
    styleUrl: './dialog.component.scss'
})
export class DialogComponent {
  @Input() name = '';
  @Output() open: EventEmitter<any> = new EventEmitter();
  @Output() close: EventEmitter<any> = new EventEmitter();

  @ViewChild('dialog', { static: true }) dialog: ElementRef | undefined;

  showDialog() {
    this.dialog?.nativeElement.showModal();
    this.open.emit(this.dialog?.nativeElement.open);
  }

  closeDialog() {
    this.dialog?.nativeElement.close();
    this.close.emit();
  }
}
