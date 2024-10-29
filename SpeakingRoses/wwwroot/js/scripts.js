document.addEventListener('DOMContentLoaded', () => {
    const baseUrl = window.location.origin;
    const apiUrl = `${baseUrl}/api/Tasks`;

    const taskForm = document.getElementById('taskForm');
    const createTaskButton = document.getElementById('createTaskButton');
    const updateTaskButton = document.getElementById('updateTaskButton');
    const cancelEditButton = document.getElementById('cancelEditButton');
    const tasksTable = document.getElementById('tasksTable').getElementsByTagName('tbody')[0];
    const filterPriority = document.getElementById('filterPriority');
    const filterStatus = document.getElementById('filterStatus');
    const filterTasksButton = document.getElementById('filterTasksButton');

    let currentTaskId = null;
    let allTasks = [];

    const priorityMap = {
        "Low": 0,
        "Medium": 1,
        "High": 2
    };

    const statusMap = {
        "Pending": 0,
        "Completed": 1
    };

    // Load tasks
    async function loadTasks() {
        try {
            const response = await fetch(apiUrl);
            if (!response.ok) {
                throw new Error(`Failed to load tasks: ${response.statusText}`);
            }
            allTasks = await response.json();
            displayTasks(allTasks);
        } catch (error) {
            console.error('Error:', error);
        }
    }

    // Display tasks
    function displayTasks(tasks) {
        tasksTable.innerHTML = '';
        tasks.forEach(task => {
            const row = tasksTable.insertRow();
            row.innerHTML = `
                <td>${task.title}</td>
                <td>${task.description}</td>
                <td>${Object.keys(priorityMap).find(key => priorityMap[key] === task.priority)}</td>
                <td>${task.dueDate.split('T')[0]}</td>
                <td>${Object.keys(statusMap).find(key => statusMap[key] === task.status)}</td>
                <td class="actions">
                    <button class="btn-edit" onclick="editTask(${task.id})">Edit</button>
                    <button class="btn-delete" onclick="deleteTask(${task.id})">Delete</button>
                    <button class="btn-status" onclick="toggleTaskStatus(${task.id}, ${task.status})">${task.status === statusMap["Pending"] ? 'Mark as Completed' : 'Mark as Pending'}</button>
                </td>
            `;
        });
    }

    // Filter tasks
    function filterTasks() {
        const selectedPriority = filterPriority.value;
        const selectedStatus = filterStatus.value;

        let filteredTasks = allTasks;

        if (selectedPriority !== "All") {
            filteredTasks = filteredTasks.filter(task => {
                return Object.keys(priorityMap).find(key => priorityMap[key] === task.priority) === selectedPriority;
            });
        }

        if (selectedStatus !== "All") {
            filteredTasks = filteredTasks.filter(task => {
                return Object.keys(statusMap).find(key => statusMap[key] === task.status) === selectedStatus;
            });
        }

        displayTasks(filteredTasks);
    }

    // Create task
    taskForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (currentTaskId === null) {
            const formData = new FormData(taskForm);
            const task = {
                title: formData.get('title'),
                description: formData.get('description'),
                priority: priorityMap[formData.get('priority')],
                dueDate: formData.get('dueDate'),
                status: statusMap[formData.get('status')]
            };

            console.log('Creating task with data:', JSON.stringify(task));

            try {
                const response = await fetch(apiUrl, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(task)
                });

                if (!response.ok) {
                    const errorMessage = await response.text();
                    throw new Error(`Error: ${errorMessage}`);
                }

                taskForm.reset();
                loadTasks();
            } catch (error) {
                console.error('Failed to create task:', error);
            }
        }
    });

    // Update task
    updateTaskButton.addEventListener('click', async () => {
        const formData = new FormData(taskForm);
        const updatedTask = {
            id: currentTaskId,
            title: formData.get('title'),
            description: formData.get('description'),
            priority: priorityMap[formData.get('priority')],
            dueDate: formData.get('dueDate'),
            status: statusMap[formData.get('status')]
        };

        console.log('Updating task with data:', JSON.stringify(updatedTask));

        try {
            const response = await fetch(`${apiUrl}/${currentTaskId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updatedTask)
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                throw new Error(`Error: ${errorMessage}`);
            }

            taskForm.reset();
            loadTasks();
            currentTaskId = null;
            createTaskButton.style.display = 'inline-block';
            updateTaskButton.style.display = 'none';
            cancelEditButton.style.display = 'none';
        } catch (error) {
            console.error('Failed to update task:', error);
        }
    });

    // Cancel edit
    cancelEditButton.addEventListener('click', () => {
        currentTaskId = null;
        taskForm.reset();
        createTaskButton.style.display = 'inline-block';
        updateTaskButton.style.display = 'none';
        cancelEditButton.style.display = 'none';
    });

    // Edit task
    window.editTask = async (id) => {
        try {
            const response = await fetch(`${apiUrl}/${id}`);
            if (!response.ok) {
                throw new Error(`Failed to load task for editing: ${response.statusText}`);
            }
            const task = await response.json();

            currentTaskId = id;
            document.getElementById('title').value = task.title;
            document.getElementById('description').value = task.description;
            document.getElementById('priority').value = Object.keys(priorityMap).find(key => priorityMap[key] === task.priority);
            document.getElementById('dueDate').value = task.dueDate.split('T')[0];
            document.getElementById('status').value = Object.keys(statusMap).find(key => statusMap[key] === task.status);

            createTaskButton.style.display = 'none';
            updateTaskButton.style.display = 'inline-block';
            cancelEditButton.style.display = 'inline-block';
        } catch (error) {
            console.error('Failed to load task for editing:', error);
        }
    };

    // Delete task
    window.deleteTask = async (id) => {
        try {
            const response = await fetch(`${apiUrl}/${id}`, {
                method: 'DELETE'
            });
            if (!response.ok) {
                throw new Error(`Failed to delete task: ${response.statusText}`);
            }
            loadTasks();
        } catch (error) {
            console.error('Failed to delete task:', error);
        }
    };

    // Toggle task status
    window.toggleTaskStatus = async (id, currentStatus) => {
        try {
            const task = allTasks.find(task => task.id === id);
            task.status = currentStatus === statusMap["Pending"] ? statusMap["Completed"] : statusMap["Pending"];

            const response = await fetch(`${apiUrl}/${id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(task)
            });

            if (!response.ok) {
                const errorMessage = await response.text();
                throw new Error(`Error: ${errorMessage}`);
            }

            loadTasks();
        } catch (error) {
            console.error('Failed to toggle task status:', error);
        }
    };

    // Filter tasks
    filterTasksButton.addEventListener('click', filterTasks);

    // Initial load
    loadTasks();
});