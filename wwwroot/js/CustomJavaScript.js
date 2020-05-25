function confirmDelete(uniqueId, isDeleteSelected) {
    var deleteSpan = 'deleteSpan_' + uniqueId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + uniqueId;

    if (isDeleteSelected) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    }

    else {
        $('#' + confirmDeleteSpan).hide();
        $('#' + deleteSpan).show();
    }
}