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
	<router-link to="/config" class="btn btn-secondary ml-2">Back</router-link>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Backend from '@/service/backend';
import router from '@/router';
import { Ref } from 'vue-property-decorator';
import { Action } from 'vuex-class';

@Options({
	name: 'AddCurrencyView',
})
export default class AddCurrencyView extends Vue {
	@Ref('code')
	codeInput!: HTMLInputElement;

	@Ref('format')
	formatInput!: HTMLInputElement;

	@Action('fetchState')
	fetchState!: () => void;

	async onclick() {
		const result = await Backend.tryFetch(
			Backend.state().currency.currencyCreate({
				code: this.codeInput.value,
				format: this.formatInput.value,
			})
		);
		if (result?.ok) {
			this.fetchState();
			await router.push('/config');
		} else {
			alert(`Failed: ${result?.error}`);
		}
	}
}
</script>
