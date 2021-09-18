const selectElementText = (elementId: string) => {
    const el = document.getElementById(elementId) as HTMLInputElement;

    if (el.select) {
        el.select();
    }
}