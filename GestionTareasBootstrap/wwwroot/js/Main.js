// Esperar a que el DOM esté completamente cargado
document.addEventListener("DOMContentLoaded", () => {
    // Obtener elementos del DOM
    const botonAbrirModal = document.getElementById("openModalBtn");
    const modalTareaElemento = document.getElementById("taskModal");

    // Verificar si los elementos existen antes de agregar eventos
    if (botonAbrirModal && modalTareaElemento) {
        // Crear una instancia del modal de Bootstrap
        let modalTarea = new bootstrap.Modal(modalTareaElemento);

        // Mostrar el modal al hacer clic en el botón
        botonAbrirModal.addEventListener("click", () => {
            modalTarea.show();
        });

        // Cargar usuarios cuando se muestra el modal
        modalTareaElemento.addEventListener("show.bs.modal", async () => {
            await cargarUsuarios();
        });
    }

    // Configurar Drag and Drop para actualizar el estado de la tarea
    const columnas = document.querySelectorAll('.column');
    columnas.forEach(columna => {
        // Permitir el arrastre sobre las columnas
        columna.addEventListener('dragover', e => {
            e.preventDefault();
            e.dataTransfer.dropEffect = "move";
        });

        // Manejar el evento de soltar para actualizar el estado de la tarea
        columna.addEventListener('drop', async e => {
            e.preventDefault();
            const idTarea = e.dataTransfer.getData('taskId');

            if (!idTarea || idTarea === "0") {
                console.error("ID de tarea no válido en el evento drop:", idTarea);
                return;
            }

            const tarea = document.querySelector(`[data-id="${idTarea}"]`);

            if (tarea && columna.id) {
                columna.appendChild(tarea);
                await actualizarTareaEstado(idTarea, columna.id);
                location.reload();
            }
        });
    });

    // Configurar eventos de arrastre para las tarjetas de tareas
    document.querySelectorAll('.card').forEach(tarea => {
        tarea.addEventListener('dragstart', e => {
            const idTarea = tarea.dataset.id;

            if (!idTarea || idTarea === "0") {
                console.error("Error: Tarea sin ID válido al iniciar drag.", tarea);
                return;
            }
            e.dataTransfer.setData('taskId', idTarea);
            e.dataTransfer.effectAllowed = "move";
        });
    });

    // Asociar eventos al cargar la página
    document.getElementById("editTaskForm").addEventListener("submit", actualizarTarea);

    // Configurar el botón de confirmar eliminación
    document.getElementById('confirmarEliminar').addEventListener('click', async () => {
        if (tareaIdAEliminar !== null) {
            await eliminarTarea(tareaIdAEliminar);
            $('#modalConfirmacion').modal('hide');
        }
    });
    document.getElementById('cerrarModalConfirmar').addEventListener('click', async () => {
        if (tareaIdAEliminar !== null) {
            $('#modalConfirmacion').modal('hide');
        }
    });
    document.getElementById('cerrarConIcono').addEventListener('click', async () => {
        if (tareaIdAEliminar !== null) {
            $('#modalConfirmacion').modal('hide');
        }
    });
});

// Función para mostrar el spinner
const mostrarSpinner = () => {
    document.getElementById("spinner").style.display = "flex";
}

// Función para ocultar el spinner
const ocultarSpinner = () => {
    document.getElementById("spinner").style.display = "none";
}

// Función para mostrar una alerta de éxito
const mostrarAlerta = (mensaje) => {
    const alertBox = document.getElementById('alertBox');
    const alertMessage = document.getElementById('alertMessage');

    alertMessage.textContent = mensaje;
    alertBox.style.display = 'block';

    setTimeout(() => {
        alertBox.style.display = 'none';
        location.reload();
        return true;
    }, 1000); // 1000 milisegundos = 1 segundo
}

// Función para mostrar una alerta de error
const mostrarAlertaError = (mensaje) => {
    const alertBox = document.getElementById('alertBoxDanger');
    const alertMessage = document.getElementById('alertMessageDanger');

    alertMessage.textContent = mensaje;
    alertBox.style.display = 'block';

    setTimeout(() => {
        alertBox.style.display = 'none';
        location.reload();
        return true;
    }, 2000); // 2000 milisegundos = 2 segundos
}

// Función para actualizar el estado de una tarea al moverla de columna
const actualizarTareaEstado = async (idTarea, estado) => {
    try {
        const numero = parseInt(idTarea);
        const tarea = {
            Id_Tarea: numero,
            Estado: estado
        };
        mostrarSpinner();
        const response = await fetch('/Main/ActualizarEstadoTarea', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(tarea) // Enviando el objeto JSON
        });

        if (!response.ok) {
            throw new Error(`Error al actualizar la tarea: ${response.status}`);
        }
    } catch (error) {
        console.error('Error al actualizar la tarea:', error);
    } finally {
        ocultarSpinner();
    }
}

// Cargar usuarios y llenar la lista de asignación de tareas
const cargarUsuarios = async () => {
    const contenedorUsuarios = document.getElementById("AssignedTo");

    if (!contenedorUsuarios) {
        console.error("No se encontró el contenedor de usuarios.");
        return;
    }

    try {
        mostrarSpinner();
        const response = await fetch("/User/ObtenerUsuarios");

        if (!response.ok) {
            throw new Error(`Error en la respuesta: ${response.status} ${response.statusText}`);
        }

        const usuarios = await response.json();
        llenarUsuariosAsignados(usuarios);
    } catch (error) {
        console.error("Error al cargar usuarios:", error);
    } finally {
        ocultarSpinner();
    }
}

// Llenar la lista de usuarios asignados a una tarea
const llenarUsuariosAsignados = (usuarios) => {
    const contenedorUsuarios = document.getElementById("AssignedTo");
    contenedorUsuarios.innerHTML = "";

    if (usuarios.length === 0) {
        const mensajeNoUsuarios = document.createElement("div");
        mensajeNoUsuarios.textContent = "No hay usuarios disponibles";
        mensajeNoUsuarios.classList.add("form-text");
        contenedorUsuarios.appendChild(mensajeNoUsuarios);
        return;
    }

    usuarios.forEach(usuario => {
        const contenedorCheckbox = document.createElement("div");
        contenedorCheckbox.classList.add("form-check");

        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.classList.add("form-check-input");
        checkbox.id = `user_${usuario.id_Usuario}`;
        checkbox.name = "AssignedTo[]";
        checkbox.value = usuario.id_Usuario;

        const etiqueta = document.createElement("label");
        etiqueta.classList.add("form-check-label");
        etiqueta.htmlFor = `user_${usuario.id_Usuario}`;
        etiqueta.textContent = usuario.nombre;

        contenedorCheckbox.appendChild(checkbox);
        contenedorCheckbox.appendChild(etiqueta);
        contenedorUsuarios.appendChild(contenedorCheckbox);
    });
    ocultarSpinner();
}

let tareaIdAEliminar = null;

// Función para abrir el modal de confirmación
const abrirModalConfirmacion = (id) => {
    tareaIdAEliminar = id;
    $('#modalConfirmacion').modal('show');
}

// Función para eliminar una tarea
const eliminarTarea = async (id) => {
    try {
        mostrarSpinner();
        const response = await fetch(`/Main/EliminarTarea/${id}`, {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            const data = await response.json();
            mostrarAlerta('Tarea eliminada correctamente.');
            window.location.href = data.redirectUrl;
        } else if (response.status === 404) {
            mostrarAlertaError('La tarea no fue encontrada.');
        } else if (response.status === 500) {
            mostrarAlertaError('Error interno del servidor al eliminar la tarea.');
        } else {
            mostrarAlertaError(`Error desconocido: ${response.status}`);
        }
    } catch (error) {
        console.error('Error al eliminar la tarea:', error);
        mostrarAlertaError('Ocurrió un error al intentar eliminar la tarea.');
    } finally {
        ocultarSpinner();
    }
}

// Obtener todos los usuarios
const obtenerUsuariosCompletos = async () => {
    mostrarSpinner();
    const response = await fetch("/User/ObtenerUsuarios");

    if (!response.ok) {
        throw new Error(`Error en la respuesta: ${response.status} ${response.statusText}`);
    }
    const usuarios = await response.json();
    return usuarios;
}

// Función para editar una tarea
const editarTarea = async (tarea) => {
    document.getElementById("editTaskId").value = tarea.id_Tarea;
    document.getElementById("editTitle").value = tarea.titulo;
    document.getElementById("editDescription").value = tarea.descripcion;
    document.getElementById("editDueDate").value = tarea.fechaVencimiento ? tarea.fechaVencimiento.split("T")[0] : "";
    document.getElementById("editPriority").value = tarea.prioridad;
    document.getElementById("editEstado").value = tarea.estado;

    // Obtener todos los usuarios
    try {
        const todosUsuarios = await obtenerUsuariosCompletos();
        const assignedToDiv = document.getElementById("editAssignedTo");
        assignedToDiv.innerHTML = ""; 

        todosUsuarios.forEach(usuario => {
            // Verificar si el usuario está asignado a la tarea
            const estaAsignado = tarea.usuariosAsignados?.some(asignado => asignado.id_Usuario === usuario.id_Usuario);

            // Crear un checkbox para cada usuario
            const checkbox = document.createElement("div");
            checkbox.className = "form-check";
            checkbox.innerHTML = `
                <input class="form-check-input" type="checkbox" name="AssignedTo[]" value="${usuario.id_Usuario}" id="user-${usuario.id_Usuario}" ${estaAsignado ? "checked" : ""}>
                <label class="form-check-label" for="user-${usuario.id_Usuario}">
                    ${usuario.nombre}
                </label>
            `;
            assignedToDiv.appendChild(checkbox);
        });
    } catch (error) {
        console.error("Error al obtener usuarios:", error);
    } finally {
        ocultarSpinner();
    }

    // Mostrar el modal solo si existe en el DOM
    const modalElement = document.getElementById("editTaskModal");
    if (modalElement) {
        const editTaskModal = new bootstrap.Modal(modalElement);
        editTaskModal.show();
    } else {
        console.error("El modal no fue encontrado en el DOM");
    }
};

// Función para actualizar la tarea
const actualizarTarea = async (event) => {
    event.preventDefault(); // Evita la recarga de la página
    const formData = obtenerDatosFormulario();

    try {
        mostrarSpinner();
        const response = await enviarActualizacion(formData);
        const result = await response.json();

        if (response.ok) {
            mostrarAlerta("Tarea actualizada correctamente");
            location.reload(); // Recargar la página para reflejar cambios
        } else {
            mostrarError(result.errors);
        }
    } catch (error) {
        console.error("Error en la actualización:", error);
        mostrarError(["Error al actualizar la tarea."]);
    } finally {
        ocultarSpinner();
    }
};

// Obtener datos del formulario
const obtenerDatosFormulario = () => {
    const form = document.getElementById("editTaskForm");
    const formData = new FormData(form);

    // Obtener los usuarios asignados
    document.querySelectorAll("input[name='AssignedTo[]']:checked").forEach(checkbox => {
        formData.append("AssignedTo", checkbox.value);
    });

    // Obtener los usuarios no asignados
    document.querySelectorAll("input[name='AssignedTo[]']:not(:checked)").forEach(checkbox => {
        formData.append("NoAssignedTo", checkbox.value);
    });

    return formData;
};

// Obtener usuarios seleccionados o no seleccionados
const obtenerUsuariosSeleccionados = (seleccionados) => {
    const selector = seleccionados ? ":checked" : ":not(:checked)";
    return Array.from(document.querySelectorAll(`input[name="AssignedTo[]"]${selector}`))
        .map(checkbox => checkbox.value);
};

// Enviar la actualización al servidor
const enviarActualizacion = async (formData) => {
    return await fetch("/Main/actualizarTarea", {
        method: "POST",
        body: formData
    });
};

// Mostrar errores en el formulario
const mostrarError = (errores) => {
    document.getElementById("editTaskError").innerText = errores.join(", ");
};