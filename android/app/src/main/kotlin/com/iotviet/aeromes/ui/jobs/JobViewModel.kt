package com.iotviet.aeromes.ui.jobs

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.model.Job
import com.iotviet.aeromes.domain.repository.WorkRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class JobListUiState(
    val jobs: List<Job> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null
)

data class JobDetailUiState(
    val job: Job? = null,
    val isLoading: Boolean = false,
    val isFinishing: Boolean = false,
    val error: String? = null,
    val finishSuccess: Boolean = false
)

@HiltViewModel
class JobViewModel @Inject constructor(
    private val workRepository: WorkRepository,
    savedStateHandle: SavedStateHandle
) : ViewModel() {

    private val _uiState = MutableStateFlow(JobListUiState())
    val uiState = _uiState.asStateFlow()

    private val _detailState = MutableStateFlow(JobDetailUiState())
    val detailState = _detailState.asStateFlow()

    init {
        loadJobs()
        savedStateHandle.get<Long>("jobId")?.let { loadDetail(it) }
    }

    fun refresh() = loadJobs()

    private fun loadJobs() {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }
            workRepository.getJobs()
                .onSuccess { jobs -> _uiState.update { it.copy(jobs = jobs, isLoading = false) } }
                .onFailure { e -> _uiState.update { it.copy(isLoading = false, error = e.message) } }
        }
    }

    fun loadDetail(jobId: Long) {
        viewModelScope.launch {
            _detailState.update { it.copy(isLoading = true, error = null) }
            workRepository.getJob(jobId)
                .onSuccess { job -> _detailState.update { it.copy(job = job, isLoading = false) } }
                .onFailure { e -> _detailState.update { it.copy(isLoading = false, error = e.message) } }
        }
    }

    fun finishJob(jobId: Long) {
        viewModelScope.launch {
            _detailState.update { it.copy(isFinishing = true) }
            workRepository.finishJob(jobId)
                .onSuccess { _detailState.update { it.copy(isFinishing = false, finishSuccess = true) } }
                .onFailure { e -> _detailState.update { it.copy(isFinishing = false, error = e.message) } }
        }
    }
}
