package com.iotviet.aeromes.ui.sop

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.model.SopDocument
import com.iotviet.aeromes.domain.model.SopDocumentDetail
import com.iotviet.aeromes.domain.repository.SopRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class SopListUiState(
    val query: String = "",
    val documents: List<SopDocument> = emptyList(),
    val isLoading: Boolean = false,
    val error: String? = null
)

data class SopDetailUiState(
    val document: SopDocumentDetail? = null,
    val isLoading: Boolean = false,
    val error: String? = null
)

@HiltViewModel
class SopListViewModel @Inject constructor(
    private val sopRepository: SopRepository,
    savedStateHandle: SavedStateHandle
) : ViewModel() {

    private val _uiState = MutableStateFlow(SopListUiState())
    val uiState = _uiState.asStateFlow()

    init {
        val woId = savedStateHandle.get<String>("woId")
        if (!woId.isNullOrBlank()) {
            _uiState.update { it.copy(query = woId) }
            search(woId)
        }
    }

    fun onQueryChange(value: String) = _uiState.update { it.copy(query = value) }

    fun search(query: String = _uiState.value.query.trim()) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }
            sopRepository.getSopDocuments(productCode = query.takeIf { it.isNotBlank() })
                .onSuccess { docs -> _uiState.update { it.copy(documents = docs, isLoading = false) } }
                .onFailure { e -> _uiState.update { it.copy(isLoading = false, error = e.message) } }
        }
    }
}

@HiltViewModel
class SopDetailViewModel @Inject constructor(
    private val sopRepository: SopRepository
) : ViewModel() {

    private val _uiState = MutableStateFlow(SopDetailUiState())
    val uiState = _uiState.asStateFlow()

    fun load(id: String) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }
            sopRepository.getSopDocument(id)
                .onSuccess { doc -> _uiState.update { it.copy(document = doc, isLoading = false) } }
                .onFailure { e -> _uiState.update { it.copy(isLoading = false, error = e.message) } }
        }
    }
}
