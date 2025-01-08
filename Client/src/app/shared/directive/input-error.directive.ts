import { Directive, Input, ElementRef, Renderer2, OnInit } from '@angular/core';
import { AbstractControl, FormControl, NgControl } from '@angular/forms';

@Directive({
  standalone: true,
  selector: '[appInputError]'
})
export class InputErrorDirective implements OnInit {

  @Input() control!: FormControl;

  constructor(
    private elementRef: ElementRef,
    private renderer: Renderer2,
    private ngControl: NgControl
  ) { }

  ngOnInit() {
    const control = this.ngControl.control as AbstractControl;
    if (control) {
      control.statusChanges.subscribe(() => {
        const isInvalidAndDirtyOrTouched = control.invalid && (control.dirty || control.touched);
        this.renderer.addClass(this.elementRef.nativeElement, 'input-error');
        if (!isInvalidAndDirtyOrTouched) {
          this.renderer.removeClass(this.elementRef.nativeElement, 'input-error');
        }
      });
    }
  }
}