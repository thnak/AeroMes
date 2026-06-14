package com.iotviet.aeromes.ui.production

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.iotviet.aeromes.domain.repository.WorkRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

data class ProductionLogUiState(
    val qtyOK: String = "",
    val qtyNG: String = "",
    val notes: String = "",
    val isLoading: Boolean = false,
    val error: String? = null,
    val isSuccess: Boolean = false
)

@HiltViewModel
class ProductionLogViewModel @Inject constructor(
    private val workRepository: WorkRepository,
    savedStateHandle: SavedStateHandle
) : ViewModel() {

    private val jobId: Long = checkNotNull(savedStateHandle["jobId"])

    private val _uiState = MutableStateFlow(ProductionLogUiState())
    val uiState = _uiState.asStateFlow()

    fun onQtyOKChange(v: String) = _uiState.update { it.copy(qtyOK = v.filter(Char::isDigit), error = null) }
    fun onQtyNGChange(v: String) = _uiState.update { it.copy(qtyNG = v.filter(Char::isDigit), error = null) }
    fun onNotesChange(v: String) = _uiState.update { it.copy(notes = v) }

    fun submit() {
        val s = _uiState.value
        val ok = s.qtyOK.toIntOrNull() ?: 0
        val ng = s.qtyNG.toIntOrNull() ?: 0
        if (ok == 0 && ng == 0) {
            _uiState.update { it.copy(error = "Enter at least one quantity.") }
            return
        }
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, error = null) }
            workRepository.logProduction(jobId, ok, ng, s.notes.ifBlank { null })
                .onSuccess { _uiState.update { it.copy(isLoading = false, isSuccess = true) } }
                .onFailure { e -> _uiState.update { it.copy(isLoading = false, error = e.message) } }
        }
    }
}
