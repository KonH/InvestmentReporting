<template>
	<h1>Add Currency</h1>
	<div class="form-group">
		<label>
			Code:
			<input ref="code" type="text" class="form-control" />
		</label>
	</div>
	<div class="form-group">
		<label>
			Format:
			<input ref="format" type="text" class="form-control" />
		</label>
	</div>
	<button :onclick="onclick" class="btn btn-primary">Add</button>
	<router-link to="/" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';

export default class AddCurrency extends Vue {
	codeInput: HTMLInputElement | undefined;
	formatInput: HTMLInputElement | undefined;

	mounted() {
		this.codeInput = this.$refs.code as HTMLInputElement;
		this.formatInput = this.$refs.format as HTMLInputElement;
	}

	async onclick() {
		if (!this.codeInput || !this.formatInput) {
			console.error(
				`invalid setup (codeInput: ${this.codeInput}, formatInput: ${this.formatInput})`
			);
			return;
		}
		const result = await Backend.tryFetch(
			Backend.state().currency.currencyCreate({
				code: this.codeInput.value,
				format: this.formatInput.value,
			})
		);
		if (result?.ok) {
			await router.push('/');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
