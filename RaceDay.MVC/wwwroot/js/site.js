// Small progressive-enhancement touches - nothing here is required for the app
// to work, it just makes the forms a bit nicer to use.

// Preview the chosen banner/profile picture before it's uploaded.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-image-preview]').forEach(function (input) {
        input.addEventListener('change', function () {
            var targetId = input.getAttribute('data-image-preview');
            var target = document.getElementById(targetId);
            if (!target || !input.files || !input.files[0]) return;

            var reader = new FileReader();
            reader.onload = function (e) { target.src = e.target.result; };
            reader.readAsDataURL(input.files[0]);
        });
    });

    // Simple "are you sure" on delete forms.
    document.querySelectorAll('[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            if (!confirm(form.getAttribute('data-confirm'))) {
                e.preventDefault();
            }
        });
    });
});
